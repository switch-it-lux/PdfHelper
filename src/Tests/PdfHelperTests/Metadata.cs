using Sitl.Pdf.Tests.Resources;
using Xunit.Abstractions;

namespace Sitl.Pdf.Tests.PdfHelperTests {

    public class Metadata(ITestOutputHelper output) : TestBase(output) {

        [Fact]
        public void AddMetadataToNewPdf() {
            var fileName = GetTestFileName();

            using (var pdfHelper = new PdfHelper(PageSizes.A4)) {
                Output.WriteLine($"Setting metadata to new PDF...");
                pdfHelper.SetMetadata("Author", "AuthorValue");
                pdfHelper.SetMetadata("Custom", "CustomValue");

                Output.WriteLine($"Saving PDF file to '{fileName}'...");
                pdfHelper.Save(fileName);
            }

            Output.WriteLine($"Opening PDF file '{fileName}'...");
            using (var pdfHelper = new PdfHelper(fileName)) {
                Output.WriteLine($"Getting metadata...");
                var a = pdfHelper.GetMetadata("Author");
                var c = pdfHelper.GetMetadata("Custom");
                Assert.Equal("AuthorValue", a);
                Assert.Equal("CustomValue", c);
            }
        }

        [Fact]
        public void AddMetadataToSamplePdf1() {
            var fileName = GetTestFileName();

            Output.WriteLine($"Loading PDF resource 'SamplePdf1.pdf'...");
            using (var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"))) {
                Output.WriteLine($"Setting metadata...");
                pdfHelper.SetMetadata(new Dictionary<string, string> {
                        { "Author", "AuthorValue" },
                        { "Custom", "CustomValue" }
                    });

                Output.WriteLine($"Saving PDF file to '{fileName}'...");
                pdfHelper.Save(fileName);
            }

            Output.WriteLine($"Opening PDF file '{fileName}'...");
            using (var pdfHelper = new PdfHelper(fileName)) {
                Output.WriteLine($"Getting metadata...");
                var dic = pdfHelper.GetMetadata();
                Assert.Equal("AuthorValue", dic["Author"]);
                Assert.Equal("CustomValue", dic["Custom"]);
            }
        }
    }
}