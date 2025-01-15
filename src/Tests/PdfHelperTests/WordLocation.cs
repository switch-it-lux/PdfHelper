using Sit.Pdf.Tests.Resources;
using Xunit.Abstractions;

namespace Sit.Pdf.Tests.PdfHelperTests {

    public class WordLocation(ITestOutputHelper output) : TestBase(output) {

        [Theory]
        [InlineData(["torquent", 8])]
        [InlineData(["TORQUENT", 8])]
        [InlineData(["Abcdef", 0])]
        public void SearchWordInSamplePdf1(string word, int expectedNumber) {
            using var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"));
            var res = pdfHelper.GetWordLocations(word, true);
            Assert.Equal(expectedNumber, res.Length);
        }

        [Theory]
        [InlineData([@"Torqu(.*)t", 0])]
        [InlineData([@"(?i)Torqu(.*)t", 8])]
        public void SearchRegexInSamplePdf1(string pattern, int expectedNumber) {
            using var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"));
            var res = pdfHelper.GetRegexLocations(pattern);
            Assert.Equal(expectedNumber, res.Length);
        }
    }
}