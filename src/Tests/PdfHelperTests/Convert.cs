using Sitl.Pdf.Tests.Resources;
using Xunit.Abstractions;

namespace Sitl.Pdf.Tests.PdfHelperTests {

    public class Convert(ITestOutputHelper output) : TestBase(output) {

        [Theory]
        [InlineData(72)]
        [InlineData(96)]
        [InlineData(300)]
        public void ConvertSamplePdf1ToBitmaps(int dpi) {
            string[] bitmapFileNames;

            Output.WriteLine($"Loading PDF resource 'SamplePdf1.pdf'...");
            using (var pdfHelper = new PdfHelper(ResourceLoader.ReadAsBytes("SamplePdf1.pdf"))) {
                Assert.Equal(3, pdfHelper.GetNumberOfPages());
                for (int i = 1; i <= 3; i++) {
                    Assert.Equal(PageSizes.Letter, pdfHelper.GetPageSize(i));
                }

                Output.WriteLine($"Converting ({dpi}dpi)...");
                var res = pdfHelper.ConvertToBitmaps(1, 3, dpi);
                Assert.Equal(3, res.Length);

                if (dpi == 72) {
                    Assert.Equal(res[0].BitmapWidth, res[0].PageWidth);
                    Assert.Equal(res[0].BitmapHeight, res[0].PageHeight);
                } else if (dpi > 72) {
                    Assert.True(res[0].BitmapWidth > res[0].PageWidth);
                    Assert.True(res[0].BitmapHeight > res[0].PageHeight);
                } else {
                    Assert.True(res[0].BitmapWidth < res[0].PageWidth);
                    Assert.True(res[0].BitmapHeight < res[0].PageHeight);
                }

                bitmapFileNames = res.Select(x => GetTestFileName($"-p{x.Page} ({dpi}dpi)", "bmp")).ToArray();
                for (int i = 0; i < res.Length; i++) {
                    Output.WriteLine($"Saving bitmap file to '{bitmapFileNames[i]}'...");
                    res[i].Bitmap.Save(bitmapFileNames[i]);
                }
            }

            Output.WriteLine($"New PDF file from bitmap files...");
            using (var pdfHelper = PdfHelper.FromImages(bitmapFileNames, 0, 72f / dpi)) {
                Assert.Equal(3, pdfHelper.GetNumberOfPages());
                for (int i = 1; i <= 3; i++) {
                    Assert.Equal(PageSizes.Letter.Width, pdfHelper.GetPageSize(i).Width, 1f);
                    Assert.Equal(PageSizes.Letter.Height, pdfHelper.GetPageSize(i).Height, 1f);
                }

                var fileName = GetTestFileName($" ({dpi}dpi)");
                Output.WriteLine($"Saving bitmap file to '{fileName}'...");
                pdfHelper.Save(fileName);
            }
        }

        [Fact]
        public void ConvertFromImages() {
            using var pdfHelper = PdfHelper.FromImages([ResourceLoader.ReadAsBytes("SampleImage1.jpg"), ResourceLoader.ReadAsBytes("SampleImage2.jpg")]);

            Assert.Equal(2, pdfHelper.GetNumberOfPages());
            Assert.Equal(pdfHelper.GetPageSize(1).Width, pdfHelper.GetPageSize(1).Height);
            Assert.Equal(new PageSize(800, 534), pdfHelper.GetPageSize(2));

            var fileName = GetTestFileName();
            Output.WriteLine($"Saving PDF file to '{fileName}'...");
            pdfHelper.Save(fileName);
        }

        [Fact]
        public void ConvertFromHtml() {
            using var pdfHelper = PdfHelper.FromHtml("<html><body>Test</body></html>", PageSizes.A4);

            var fileName = GetTestFileName();
            Output.WriteLine($"Saving PDF file to '{fileName}'...");
            pdfHelper.Save(fileName);
        }

        [Fact]
        public async Task ConvertFromEmailEml() {
            using var pdfHelper = await PdfHelper.FromEmailAsync(ResourceLoader.ReadAsBytes("SampleEml1.eml"), EmailType.Mime, PageSizes.A4, true);

            var fileName = GetTestFileName();
            Output.WriteLine($"Saving PDF file to '{fileName}'...");
            pdfHelper.Save(fileName);
        }

        [Fact]
        public async Task ConvertFromEmailEmlWithAttachment() {
            using var pdfHelper = await PdfHelper.FromEmailAsync(ResourceLoader.ReadAsBytes("SampleEml2.eml"), EmailType.Mime, PageSizes.A4, true);

            var fileName = GetTestFileName();
            Output.WriteLine($"Saving PDF file to '{fileName}'...");
            pdfHelper.Save(fileName);
        }

        [Fact]
        public async Task ConvertFromEmailOutlook() {
            using var pdfHelper = await PdfHelper.FromEmailAsync(ResourceLoader.ReadAsBytes("SampleMsg1.msg"), EmailType.OutlookMsg, PageSizes.A4, true);

            var fileName = GetTestFileName();
            Output.WriteLine($"Saving PDF file to '{fileName}'...");
            pdfHelper.Save(fileName);
        }

        [Fact]
        public async Task ConvertFromEmailOutlookWithAttachment() {
            using var pdfHelper = await PdfHelper.FromEmailAsync(ResourceLoader.ReadAsBytes("SampleMsg2.msg"), EmailType.OutlookMsg, PageSizes.A4, true);

            var fileName = GetTestFileName();
            Output.WriteLine($"Saving PDF file to '{fileName}'...");
            pdfHelper.Save(fileName);
        }
    }
}