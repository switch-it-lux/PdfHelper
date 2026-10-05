using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using iText.Kernel.Pdf;
using MimeKit;
using Sitl.Pdf.Tests.Resources;
using Xunit.Abstractions;

namespace Sitl.Pdf.Tests.PdfHelperTests {

    // Checks the resources that email HTML is allowed to load when converted to PDF (SSRF / local file inclusion protection).
    public class SafeResourceRetriever(ITestOutputHelper output) : TestBase(output) {

        [Theory]
        [InlineData("http://127.0.0.1/image.png")]
        [InlineData("http://localhost/image.png")]
        [InlineData("http://[::1]/image.png")]
        [InlineData("http://10.255.255.1/image.png")]
        [InlineData("http://172.16.0.1/image.png")]
        [InlineData("http://192.168.255.254/image.png")]
        [InlineData("http://169.254.169.254/latest/meta-data/")]
        [InlineData("http://[fd00::1]/image.png")]
        [InlineData("http://example.com:8080/image.png")]
        [InlineData("ftp://example.com/image.png")]
        public async Task ConvertFromEmailBlocksUrl(string url) {
            // A blocked URL is skipped immediately: no image and no network timeout (a request would wait up to 10s)
            var stopwatch = Stopwatch.StartNew();
            using var pdfHelper = await PdfHelper.FromEmailAsync(CreateEmail($"<html><body>Test<img src=\"{url}\"/></body></html>"), EmailType.Mime, PageSizes.A4);
            stopwatch.Stop();

            Assert.Equal(0, CountImages(pdfHelper.ToByteArray()));
            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5), $"Conversion took {stopwatch.Elapsed}, the URL was probably requested");
        }

        [Fact]
        public async Task ConvertFromEmailBlocksLocalResources() {
            // Local HTTP server that must never be called
            int port;
            var socket = new TcpListener(IPAddress.Loopback, 0);
            socket.Start();
            port = ((IPEndPoint)socket.LocalEndpoint).Port;
            socket.Stop();

            using var listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();
            var requestReceived = listener.GetContextAsync();

            // Local image file that must never be embedded
            var imagePath = GetTestFileName(extension: ".jpg");
            File.WriteAllBytes(imagePath, ResourceLoader.ReadAsBytes("SampleImage1.jpg"));

            var html = "<html><body>Test"
                + $"<img src=\"http://localhost:{port}/image.jpg\"/>"
                + $"<img src=\"{new Uri(imagePath).AbsoluteUri}\"/>"
                + $"<img src=\"{imagePath}\"/>"
                + "</body></html>";

            using var pdfHelper = await PdfHelper.FromEmailAsync(CreateEmail(html), EmailType.Mime, PageSizes.A4);
            pdfHelper.Save(GetTestFileName());

            Assert.Equal(0, CountImages(pdfHelper.ToByteArray()));
            var completed = await Task.WhenAny(requestReceived, Task.Delay(TimeSpan.FromSeconds(1)));
            Assert.False(completed == requestReceived, "The local HTTP server should not have been called");
        }

        [Theory]
        [InlineData("https://www.google.com/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png")]
        [InlineData("http://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png")] // http to https redirect
        public async Task ConvertFromEmailWithRemoteImage(string url) {
            // Requires Internet access
            var html = $"<html><body>Test<img src=\"{url}\"/></body></html>";

            using var pdfHelper = await PdfHelper.FromEmailAsync(CreateEmail(html), EmailType.Mime, PageSizes.A4);
            pdfHelper.Save(GetTestFileName());

            Assert.Equal(1, CountImages(pdfHelper.ToByteArray()));
        }

        private static byte[] CreateEmail(string html) {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse("sender@example.com"));
            message.To.Add(MailboxAddress.Parse("recipient@example.com"));
            message.Subject = "Test";
            message.Body = new TextPart("html") { Text = html };

            using var ms = new MemoryStream();
            message.WriteTo(ms);
            return ms.ToArray();
        }

        private static int CountImages(byte[] pdf) {
            using var pdfDoc = new PdfDocument(new PdfReader(new MemoryStream(pdf)));
            var count = 0;
            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++) {
                var xObjects = pdfDoc.GetPage(i).GetResources().GetResource(PdfName.XObject);
                if (xObjects == null) continue;
                foreach (var name in xObjects.KeySet()) {
                    if (PdfName.Image.Equals(xObjects.GetAsStream(name)?.GetAsName(PdfName.Subtype))) count++;
                }
            }
            return count;
        }
    }
}
