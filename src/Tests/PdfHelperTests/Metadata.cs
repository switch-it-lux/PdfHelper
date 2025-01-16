using Sitl.Pdf.Tests.Resources;
using Xunit.Abstractions;

namespace Sitl.Pdf.Tests.PdfHelperTests {

    public class Metadata(ITestOutputHelper output) : TestBase(output) {
        readonly Dictionary<string, object> metadataSample = new() {
            { "Author", "AuthorName" },
            { "CustomString", "Abc" },
            { "CustomDouble", 345.89 },
            { "CustomDate", new DateTime(2022, 11, 19, 14, 32, 53) }
        };

        [Fact]
        public void AddMetadataToNewPdf() {
            var fileName = GetTestFileName();

            using (var pdfHelper = new PdfHelper(PageSizes.A4)) {
                Output.WriteLine($"Setting metadata to new PDF...");
                pdfHelper.SetMetadata(metadataSample);

                Output.WriteLine($"Saving PDF file to '{fileName}'...");
                pdfHelper.Save(fileName);
            }

            Output.WriteLine($"Opening PDF file '{fileName}'...");
            using (var pdfHelper = new PdfHelper(fileName)) {
                Output.WriteLine($"Getting metadata...");
                var metadata = pdfHelper.GetMetadata();
                foreach (var key in metadataSample.Keys) {
                    Assert.Equal(metadataSample[key], metadata[key]);
                }
            }
        }

        [Fact]
        public void AddMetadataToSamplePdf1() {
            var fileName = GetTestFileName();

            Output.WriteLine($"Loading PDF resource 'SamplePdf1.pdf'...");
            using (var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"))) {
                Output.WriteLine($"Setting metadata...");
                pdfHelper.SetMetadata(metadataSample);

                Output.WriteLine($"Saving PDF file to '{fileName}'...");
                pdfHelper.Save(fileName);
            }

            Output.WriteLine($"Opening PDF file '{fileName}'...");
            using (var pdfHelper = new PdfHelper(fileName)) {
                Output.WriteLine($"Getting metadata...");
                var metadata = pdfHelper.GetMetadata();
                foreach (var key in metadataSample.Keys) {
                    Assert.Equal(metadataSample[key], metadata[key]);
                }
            }
        }
    }
}