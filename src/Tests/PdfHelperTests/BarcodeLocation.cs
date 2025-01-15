using Sitl.Pdf.Tests.Resources;
using Xunit.Abstractions;

namespace Sitl.Pdf.Tests.PdfHelperTests {

    public class BarcodeLocation(ITestOutputHelper output) : TestBase(output) {

        [Fact]
        public void SearchQrInSamplePdf2() {
            using var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf2.pdf"));
            var res = pdfHelper.GetBarcodeLocations();
            Assert.Equal(2, res.Length);
            Assert.Equal("Qrcode", res[0].BarcodeType);
            Assert.Equal("https://www.switch.lu", res[0].Text);
            Assert.Equal("Qrcode", res[1].BarcodeType);
            Assert.Equal("https://github.com", res[1].Text);
        }

        [Fact]
        public void SearchFirstQrInSamplePdf2() {
            using var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf2.pdf"));
            var res = pdfHelper.GetBarcodeLocations(stopAfterXMatch: 1);
            Assert.Single(res);
            Assert.Equal("Qrcode", res[0].BarcodeType);
            Assert.Equal("https://www.switch.lu", res[0].Text);
        }

        [Fact]
        public void SearchQrByPageInSamplePdf2() {
            using var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf2.pdf"));
            var res = pdfHelper.GetBarcodeLocations(searchPage: 1);
            Assert.Empty(res);

            res = pdfHelper.GetBarcodeLocations(searchPage: 2);
            Assert.Single(res);
            Assert.Equal("Qrcode", res[0].BarcodeType);
            Assert.Equal("https://www.switch.lu", res[0].Text);
        }
    }
}