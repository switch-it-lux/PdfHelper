using Sitl.Pdf.Tests.Resources;
using Xunit.Abstractions;

namespace Sitl.Pdf.Tests.PdfHelperTests {

    public class DataStamps(ITestOutputHelper output) : TestBase(output) {

        [Fact]
        public void SetDataStampsToSamplePdf1() {
            const string startTag = "<@sit-ds>";
            const string endTag = "</@sit-ds>";
            var fileName = GetTestFileName();

            Output.WriteLine($"Loading PDF resource 'SamplePdf1.pdf'...");
            using (var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"))) {
                Assert.Equal(3, pdfHelper.GetNumberOfPages());

                Output.WriteLine($"Adding data stamp...");
                var data = new Dictionary<string, string> { { "Key1", "Value1" }, { "Key2", "Value2" } };
                pdfHelper.AddDataStamp(data, startTag, endTag, true);

                Output.WriteLine($"Saving PDF file to '{fileName}'...");
                pdfHelper.Save(fileName);
            }

            Output.WriteLine($"Loading PDF '{fileName}'...");
            using (var pdfHelper = new PdfHelper(fileName)) {
                Assert.Equal(3, pdfHelper.GetNumberOfPages());

                Output.WriteLine($"Getting data stamp...");
                var data = pdfHelper.GetDataStamps(startTag, endTag);
                Assert.Equal(3, data.Count);
                for (int i = 1; i <= data.Count; i++) {
                    Assert.Equal("Value1", data[i]["Key1"]);
                    Assert.Equal("Value2", data[i]["Key2"]);
                    Assert.Equal(i.ToString(), data[i]["Page"]);
                }
            }
        }
    }
}