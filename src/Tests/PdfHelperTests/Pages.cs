using Sit.Pdf.Tests.Resources;
using Xunit.Abstractions;

namespace Sit.Pdf.Tests.PdfHelperTests {

    public class Pages(ITestOutputHelper output) : TestBase(output) {

        [Fact]
        public void AddAndCountPages() {
            Output.WriteLine($"Loading PDF resource 'SamplePdf1.pdf'...");
            using var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"));

            Output.WriteLine($"Count pages...");
            var nbp1 = pdfHelper.GetNumberOfPages();

            Output.WriteLine($"Add one page...");
            pdfHelper.AddBlankPage(PageSizes.A4);

            Output.WriteLine($"Count pages...");
            var nbp2 = pdfHelper.GetNumberOfPages();

            Assert.Equal(1, nbp2 - nbp1);

            Output.WriteLine($"Get page size...");
            var ps = pdfHelper.GetPageSize(nbp1 + 1);

            Assert.Equal(PageSizes.A4, ps);
        }

        [Fact]
        public void AddImagePages() {
            using var pdfHelper = new PdfHelper(PageSizes.A4);
            pdfHelper.AddImagePage(ResourceLoader.ReadAsBytes("SampleImage1.jpg"), 0f, 1f, null);
            pdfHelper.AddImagePage(ResourceLoader.ReadAsBytes("SampleImage1.jpg"), 0f, 1f, PageSizes.A4);
            pdfHelper.RemovePage(1);

            Assert.Equal(2, pdfHelper.GetNumberOfPages());
            Assert.Equal(pdfHelper.GetPageSize(1).Width, pdfHelper.GetPageSize(1).Height);
            Assert.Equal(PageSizes.A4, pdfHelper.GetPageSize(2));

            var fileName = GetTestFileName();
            Output.WriteLine($"Saving PDF file to '{fileName}'...");
            pdfHelper.Save(fileName);
        }
    }
}