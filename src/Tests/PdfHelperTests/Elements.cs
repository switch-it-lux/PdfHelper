using Xunit.Abstractions;

namespace Sitl.Pdf.Tests.PdfHelperTests {

    public class Elements(ITestOutputHelper output) : TestBase(output) {

        [Fact]
        public void CreateDummyPdf() {
            using var pdfHelper = new PdfHelper(PageSizes.A4, 3);

            Output.WriteLine($"Adding elements to page 1...");
            pdfHelper.AddElements(
                new PdfTextElement {
                    Page = 1,
                    X = PageSizes.A4.Width / 2,
                    Y = PageSizes.A4.Height / 2,
                    Anchor = PdfElementAnchor.BaselineCenter,
                    Color = System.Drawing.Color.Black,
                    FontSize = 40,
                    Text = "Dummy PDF"
                },
                new PdfTextElement {
                    Page = 1,
                    X = PageSizes.A4.Width / 2,
                    Y = PageSizes.A4.Height / 2 + 25,
                    Anchor = PdfElementAnchor.BaselineCenter,
                    Color = System.Drawing.Color.Black,
                    FontSize = 12,
                    Text = "Created with Sitl.PdfHelper"
                }
            );

            for (int p = 2; p <= 3; p++) {
                Output.WriteLine($"Adding elements to page {p}...");

                var qrCodeContent = p == 2 ? "https://www.switch.lu" : "https://github.com";
                pdfHelper.AddElements(
                    new PdfTextElement {
                        Page = p,
                        X = 50,
                        Y = 100,
                        Anchor = PdfElementAnchor.BaselineLeft,
                        Color = System.Drawing.Color.Black,
                        FontSize = 24,
                        Text = $"Dummy PDF title {p - 1}"
                    },
                    new PdfTextElement {
                        Page = p,
                        X = 50,
                        Y = 125,
                        Anchor = PdfElementAnchor.BaselineLeft,
                        Color = System.Drawing.Color.Black,
                        FontSize = 18,
                        Text = "Dummy PDF sub title"
                    },
                    new PdfQrcodeElement {
                        Page = p,
                        X = 50,
                        Y = 250,
                        Anchor = PdfElementAnchor.BaselineLeft,
                        Color = System.Drawing.Color.Black,
                        Size = 120,
                        Text = qrCodeContent
                    },
                    new PdfTextElement {
                        Page = p,
                        X = 50,
                        Y = 275,
                        Anchor = PdfElementAnchor.BaselineLeft,
                        Color = System.Drawing.Color.Black,
                        FontSize = 12,
                        Text = $"The content of the QrCode is '{qrCodeContent}'."
                    },
                    new PdfTextElement {
                        Page = p,
                        X = 50,
                        Y = PageSizes.A4.Height - 20,
                        Anchor = PdfElementAnchor.BaselineLeft,
                        Color = System.Drawing.Color.Black,
                        FontSize = 10,
                        Text = "Dummy PDF footer left"
                    },
                    new PdfTextElement {
                        Page = p,
                        X = PageSizes.A4.Width - 50,
                        Y = PageSizes.A4.Height - 20,
                        Anchor = PdfElementAnchor.BaselineRight,
                        Color = System.Drawing.Color.Black,
                        FontSize = 10,
                        Text = "Dummy PDF footer right"
                    }
                );
            }

            var fileName = GetTestFileName();
            Output.WriteLine($"Saving PDF file to '{fileName}'...");
            pdfHelper.Save(fileName);
        }

        [Fact]
        public void CreatePdfDots() {
            using var pdfHelper = new PdfHelper(PageSizes.A4);

            Output.WriteLine($"Adding dots...");
            pdfHelper.AddElements(
                new PdfDotElement {
                    Page = 1,
                    X = 6,
                    Y = 6,
                    Color = System.Drawing.Color.Black,
                    Radius = 5
                },
                new PdfDotElement {
                    Page = 1,
                    X = PageSizes.A4.Width - 6,
                    Y = 6,
                    Color = System.Drawing.Color.Black,
                    Radius = 5
                },
                new PdfDotElement {
                    Page = 1,
                    X = 6,
                    Y = PageSizes.A4.Height - 6,
                    Color = System.Drawing.Color.Black,
                    Radius = 5
                },
                new PdfDotElement {
                    Page = 1,
                    X = PageSizes.A4.Width - 6,
                    Y = PageSizes.A4.Height - 6,
                    Color = System.Drawing.Color.Black,
                    Radius = 5
                }
,
                new PdfDotElement {
                    Page = 1,
                    X = PageSizes.A4.Width / 2,
                    Y = PageSizes.A4.Height / 2,
                    Color = System.Drawing.Color.Black,
                    Radius = 5
                });

            var fileName = GetTestFileName();
            Output.WriteLine($"Saving PDF file to '{fileName}'...");
            pdfHelper.Save(fileName);
        }
    }
}