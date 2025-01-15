using Sit.Pdf.Tests.Resources;
using Xunit.Abstractions;

namespace Sit.Pdf.Tests.PdfHelperTests {

    public class Text(ITestOutputHelper output) : TestBase(output) {

        [Fact]
        public void ContainsAnyText() {
            using (var pdfHelper = new PdfHelper(PageSizes.A4)) {
                Output.WriteLine($"Check any text in new PDF...");
                var res = pdfHelper.ContainsAnyText();
                Assert.False(res);
            }

            Output.WriteLine($"Loading PDF resource 'SamplePdf1.pdf'...");
            using (var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"))) {
                Output.WriteLine($"Check any text in all document...");
                var res = pdfHelper.ContainsAnyText();
                Assert.True(res);

                Output.WriteLine($"Check any text on page 2 only...");
                res = pdfHelper.ContainsAnyText(2);
                Assert.True(res);
            }
        }

        [Fact]
        public void ApplyHocrOnSamplePdf1() {
            Output.WriteLine($"Loading PDF resource 'SampleOcredPdf1.pdf'...");
            using var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SampleOcredPdf1.pdf"));
            pdfHelper.AddHocrTexts(ResourceLoader.ReadAsBytes("SampleHocr1.hocr")/*, Color.Red*/);

            var fileName = GetTestFileName();
            Output.WriteLine($"Saving PDF file to '{fileName}'...");
            pdfHelper.Save(fileName);
        }
    }
}