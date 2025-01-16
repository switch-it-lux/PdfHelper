using Sitl.Pdf.Tests.Resources;
using Xunit.Abstractions;

namespace Sitl.Pdf.Tests.PdfMergerTests {

    public class Merge(ITestOutputHelper output) : TestBase(output) {

        [Fact]
        public void MergeSamplePdf1And2AndBlankPage() {
            using var pdfMerger = new PdfMergerHelper();
            pdfMerger.DocumentTitle = nameof(MergeSamplePdf1And2AndBlankPage);

            Output.WriteLine($"Adding 'SamplePdf1.pdf'...");
            pdfMerger.Add(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"));
            Assert.Equal(3, pdfMerger.NumberOfPages);

            Output.WriteLine($"Adding 'SamplePdf2.pdf'...");
            pdfMerger.Add(ResourceLoader.ReadAsBytes("SamplePdf2.pdf"));
            Assert.Equal(6, pdfMerger.NumberOfPages);

            Output.WriteLine($"Adding blank page...");
            pdfMerger.AddBlankPage(PageSizes.A4);
            Assert.Equal(7, pdfMerger.NumberOfPages);

            var fileName = GetTestFileName();
            Output.WriteLine($"Saving PDF file to '{fileName}'...");
            pdfMerger.Save(fileName);
        }
    }
}
