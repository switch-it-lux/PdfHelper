using Sit.Pdf.Tests.Resources;
using Xunit.Abstractions;

namespace Sit.Pdf.Tests.PdfHelperTests {

    public class Scale(ITestOutputHelper output) : TestBase(output) {

        [Fact]
        public void ScaleSamplePdf1() {
            Output.WriteLine($"Loading PDF resource 'SamplePdf1.pdf'...");
            using var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"));
            var initialPageSize = pdfHelper.GetPageSize(1);

            Output.WriteLine($"Scaling by 1/2...");
            pdfHelper.Scale(0.5f);
            var scaledPageSize = pdfHelper.GetPageSize(1);

            Assert.True(initialPageSize.Width == scaledPageSize.Width * 2);
            Assert.True(initialPageSize.Height == scaledPageSize.Height * 2);

            var fileName = GetTestFileName();
            Output.WriteLine($"Saving PDF file to '{fileName}'...");
            pdfHelper.Save(fileName);
        }
    }
}