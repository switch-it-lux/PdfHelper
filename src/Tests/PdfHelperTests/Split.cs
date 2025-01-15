using Sit.Pdf;
using Sit.Pdf.Tests.Resources;
using Xunit.Abstractions;

namespace Sit.Pdf.Tests.PdfHelperTests {

    public class Split(ITestOutputHelper output) : TestBase(output) {

        [Fact]
        public void ExtractPagesFromSamplePdf1() {
            byte[] res;

            Output.WriteLine($"Loading PDF resource 'SamplePdf1.pdf'...");
            using (var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"))) {
                Assert.Equal(3, pdfHelper.GetNumberOfPages());

                Output.WriteLine($"Extracting page range 2-3...");
                res = pdfHelper.ExtractPageRange("2-3");
            }

            Output.WriteLine($"Loading extracted pages...");
            using (var pdfHelper = new PdfHelper(res)) {
                Assert.Equal(2, pdfHelper.GetNumberOfPages());
            }
        }

        [Fact]
        public void SplitSamplePdf1ByPageNumbers() {
            IList<byte[]> res;

            Output.WriteLine($"Loading PDF resource 'SamplePdf1.pdf'...");
            using (var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"))) {
                Assert.Equal(3, pdfHelper.GetNumberOfPages());

                Output.WriteLine($"Splitting...");
                res = pdfHelper.SplitByPageNumbers([3]);
            }

            Assert.Equal(2, res.Count);

            Output.WriteLine($"Loading result 1...");
            using (var pdfHelper = new PdfHelper(res[0])) {
                Assert.Equal(2, pdfHelper.GetNumberOfPages());
            }

            Output.WriteLine($"Loading result 2...");
            using (var pdfHelper = new PdfHelper(res[1])) {
                Assert.Equal(1, pdfHelper.GetNumberOfPages());
            }
        }

        [Fact]
        public void SplitSamplePdf1BySize() {
            IList<byte[]> res;

            Output.WriteLine($"Loading PDF resource 'SamplePdf1.pdf'...");
            using (var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"))) {
                Assert.Equal(3, pdfHelper.GetNumberOfPages());

                Output.WriteLine($"Splitting...");
                res = pdfHelper.SplitByMaxSize(40000);
            }

            Assert.Equal(2, res.Count);

            Output.WriteLine($"Loading result 1...");
            using (var pdfHelper = new PdfHelper(res[0])) {
                Assert.Equal(1, pdfHelper.GetNumberOfPages());
                Assert.True(pdfHelper.EvaluateSizeOnDiskInKb() <= 40000);
            }

            Output.WriteLine($"Loading result 2...");
            using (var pdfHelper = new PdfHelper(res[1])) {
                Assert.Equal(2, pdfHelper.GetNumberOfPages());
                Assert.True(pdfHelper.EvaluateSizeOnDiskInKb() <= 40000);
            }
        }

        [Fact]
        public void SplitSinglePagePdfBySize() {
            using var pdfHelper = new PdfHelper(PageSizes.A4);
            Assert.Equal(1, pdfHelper.GetNumberOfPages());

            Output.WriteLine($"Splitting (1Kb)...");
            var res = pdfHelper.SplitByMaxSize(1000);
            Assert.Single(res);

            Output.WriteLine($"Splitting (0.1Kb)...");
            var ex = Record.Exception(() => pdfHelper.SplitByMaxSize(100));
            Assert.NotNull(ex);
        }
    }
}