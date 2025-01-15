using Sit.Pdf.Tests.Resources;
using Xunit.Abstractions;

namespace Sit.Pdf.Tests.PdfHelperTests {

    public class Merge(ITestOutputHelper output) : TestBase(output) {

        [Fact]
        public void AppendToSamplePdf1() {
            Output.WriteLine($"Loading PDF resource 'SamplePdf1.pdf'...");
            var SamplePdf1 = ResourceLoader.ReadAsBytes("SamplePdf1.pdf");
            using var pdfHelper = new PdfHelper(SamplePdf1);
            Assert.Equal(3, pdfHelper.GetNumberOfPages());

            Output.WriteLine($"Concatenating 1 with 'SamplePdf1.pdf'...");
            pdfHelper.AppendPdf(SamplePdf1);
            Output.WriteLine($"Concatenating 2 with 'SamplePdf1.pdf'...");
            pdfHelper.AppendPdf(SamplePdf1);
            Assert.Equal(9, pdfHelper.GetNumberOfPages());

            Output.WriteLine($"Concatenating 3 with 'SamplePdfInvalid.pdf'...");
            var ex = Record.Exception(() => pdfHelper.AppendPdf(ResourceLoader.ReadAsBytes("SamplePdfInvalid.pdf")));
            Assert.NotNull(ex);
        }
    }
}