using Sitl.Pdf.Tests.Resources;
using Xunit.Abstractions;

namespace Sitl.Pdf.Tests.PdfHelperTests {

    public class Constructor(ITestOutputHelper output) : TestBase(output) {

        [Fact]
        public void InstanciateFromByteArrayThenFromFile() {
            var fileName = GetTestFileName();
            Output.WriteLine($"Loading PDF resource 'SamplePdf1.pdf'...");
            using (var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"))) {
                Assert.Equal(3, pdfHelper.GetNumberOfPages());

                Output.WriteLine($"Saving PDF file to '{fileName}'...");
                pdfHelper.Save(fileName);
            }

            Output.WriteLine($"Loading PDF 'SamplePdf1.pdf' from disk...");
            using (var pdfHelper = new PdfHelper(fileName)) {
                Assert.Equal(3, pdfHelper.GetNumberOfPages());
            }
        }

        [Fact]
        public void InstanciateFromStream() {
            Output.WriteLine($"Loading PDF resource 'SamplePdf1.pdf'...");
            using var stream = ResourceLoader.ReadAsStream("SamplePdf1.pdf");
            using var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"));
            Assert.Equal(3, pdfHelper.GetNumberOfPages());
        }
    }
}