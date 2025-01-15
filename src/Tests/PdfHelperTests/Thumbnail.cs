using Sitl.Pdf.Tests.Resources;
using Xunit.Abstractions;

namespace Sitl.Pdf.Tests.PdfHelperTests {

    public class Thumbnail(ITestOutputHelper output) : TestBase(output) {

        [Fact]
        public void GenThumbnailForSamplePdf1() {
            Output.WriteLine($"Loading PDF resource 'SamplePdf1.pdf'...");
            using var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"));
            Assert.Equal(3, pdfHelper.GetNumberOfPages());

            Output.WriteLine($"Generating thumbnail...");
            var res = pdfHelper.GenerateThumbnail(1, 200, -1, true);
            Assert.NotNull(res);

            var fileName = GetTestFileName($"-p1", "png");
            Output.WriteLine($"Saving PNG file to '{fileName}'...");
            File.WriteAllBytes(fileName, res);
        }

        [Fact]
        public void SetAndGetThumbnailToSamplePdf1() {
            Output.WriteLine($"Loading PDF resource 'SamplePdf1.pdf'...");
            using var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"));
            Assert.Equal(3, pdfHelper.GetNumberOfPages());
            var png = pdfHelper.GetThumbnail();
            Assert.Null(png);

            Output.WriteLine($"Generating thumbnail...");
            png = pdfHelper.GenerateThumbnail(1, 100, -1, true);
            Assert.NotNull(png);

            Output.WriteLine($"Setting thumbnail to 'SamplePdf1.pdf'...");
            pdfHelper.SetThumbnail(png);

            Output.WriteLine($"Getting thumbnail from 'SamplePdf1.pdf'...");
            var png2 = pdfHelper.GetThumbnail();

            var fileName = GetTestFileName("", "png");
            Output.WriteLine($"Saving PNG file to '{fileName}'...");
            File.WriteAllBytes(fileName, png2);
        }
    }
}