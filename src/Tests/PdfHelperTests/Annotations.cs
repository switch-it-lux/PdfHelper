using System.Drawing;
using Sit.Pdf.Tests.Resources;
using Xunit.Abstractions;

namespace Sit.Pdf.Tests.PdfHelperTests {

    public class Annotations(ITestOutputHelper output) : TestBase(output) {

        [Fact]
        public void HighlightTitleInSamplePdf1() {
            Output.WriteLine($"Loading PDF resource 'SamplePdf1.pdf'...");
            using var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"));
            Assert.False(pdfHelper.ContainsAnyAnnotations());

            Output.WriteLine($"Searching title...");
            var title = pdfHelper.GetWordLocations("Sample", searchPage: 1).FirstOrDefault();
            Assert.NotNull(title);

            Output.WriteLine($"Highlighting title...");
            pdfHelper.HighlightArea(title.Location.ToPdfArea(), Color.Yellow);
            Assert.True(pdfHelper.ContainsAnyAnnotations());

            var fileName = GetTestFileName();
            Output.WriteLine($"Saving PDF file to '{fileName}'...");
            pdfHelper.Save(fileName);

            Output.WriteLine($"Removing annotations...");
            pdfHelper.RemoveAnnotations();
            Assert.False(pdfHelper.ContainsAnyAnnotations());
        }
    }
}