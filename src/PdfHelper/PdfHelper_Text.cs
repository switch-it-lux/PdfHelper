using System.Drawing;
using System.IO;
using System.Linq;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Extgstate;

namespace Sitl.Pdf {

    public partial class PdfHelper {

        /// <summary>
        /// Checks if the PDF document contains any text.
        /// Useful to check if a document was OCRed.
        /// </summary>
        public bool ContainsAnyText(int page = -1) {
            using (PdfReader reader = new PdfReader(PdfStream)) {
                using (PdfDocument pdfDoc = new PdfDocument(reader)) {
                    for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++) {
                        if (page <= 0 || page == i) {
                            string text = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), new SimpleTextExtractionStrategy());
                            if (!string.IsNullOrWhiteSpace(text)) {
                                text = text.Replace(" ", "").Replace("\\n", "").Replace("\\r", "");
                                if (!string.IsNullOrWhiteSpace(text)) return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Adds text in order to make the PDF document searchable. 
        /// The texts are provided using hOCR data representation (open standard for formatted text obtained from optical character recognition).
        /// </summary>
        /// <param name="hocrFilePath">The hOCR file path.</param>
        /// <param name="textColor">The text color (should be used for testing only).</param>
        public void AddHocrTexts(string hocrFilePath, Color? textColor = null) {
            using (var stream = File.OpenRead(hocrFilePath)) {
                AddHocrTexts(stream, textColor);
            }
        }

        /// <summary>
        /// Adds text in order to make the PDF document searchable. 
        /// The texts are provided using hOCR data representation (open standard for formatted text obtained from optical character recognition).
        /// </summary>
        /// <param name="hocr">The hOCR document.</param>
        /// <param name="textColor">The text color (should be used for testing only).</param>
        public void AddHocrTexts(byte[] hocr, Color? textColor = null) {
            using (var stream = new MemoryStream(hocr)) {
                AddHocrTexts(stream, textColor);
            }
        }

        /// <summary>
        /// Adds text in order to make the PDF document searchable. 
        /// The texts are provided using hOCR data representation (open standard for formatted text obtained from optical character recognition).
        /// </summary>
        /// <param name="hocr">The hOCR document.</param>
        /// <param name="textColor">The text color (should be used for testing only).</param>
        public void AddHocrTexts(Stream hocr, Color? textColor = null) {
            var hocrPages = HocrParser.Parse(hocr);

            var newPdfStream = new MemoryStream();
            try {
                using (PdfReader reader = new PdfReader(PdfStream)) {
                    using (PdfWriter writer = new PdfWriter(newPdfStream)) {
                        writer.SetCloseStream(false);
                        writer.IsFullCompression();
                        using (PdfDocument pdfDoc = new PdfDocument(reader, writer)) {
                            for (int i = 1; i < pdfDoc.GetNumberOfPages() + 1; i++) {
                                var hocrPage = hocrPages.FirstOrDefault(x => x.Page == i);
                                if (hocrPage != null && hocrPage.WordLocations.Any()) {
                                    var pdfPage = pdfDoc.GetPage(i);
                                    var pdfPageWidth = pdfPage.GetPageSize().GetWidth();

                                    var pdfFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                                    var fontScale = pdfPageWidth / hocrPage.Width;

                                    var pdfCanvas = new PdfCanvas(pdfPage);
                                    if (textColor == null)
                                        pdfCanvas.SetExtGState(new PdfExtGState().SetFillOpacity(0f));
                                    else
                                        pdfCanvas.SetColor(new iText.Kernel.Colors.DeviceRgb(textColor.Value), true);
                                    
                                    pdfCanvas.BeginText();
                                    foreach (var word in hocrPage.WordLocations) {
                                        pdfCanvas.SetFontAndSize(pdfFont, (word.FontSize > 0 ? word.FontSize : 8) * fontScale);

                                        if (word.TextAngle == 90) {
                                            pdfCanvas.SetTextMatrix(0f, 1f, -1f, 0f, (word.X + word.Width) * fontScale, pdfPage.GetPageSize().GetHeight() - word.Height * fontScale - word.Y * fontScale);
                                        } else if (word.TextAngle == 180) {
                                            pdfCanvas.SetTextMatrix(-1f, 0f, 0f, 1f, (word.X + word.Width) * fontScale, pdfPage.GetPageSize().GetHeight() - word.Height * fontScale - word.Y * fontScale);
                                        } else if (word.TextAngle == 270) {
                                            pdfCanvas.SetTextMatrix(0f, 1f, 1f, 0f, (word.X + word.Width) * fontScale, pdfPage.GetPageSize().GetHeight() - word.Height * fontScale - word.Y * fontScale);
                                        } else {
                                            pdfCanvas.SetTextMatrix(word.X * fontScale, pdfPage.GetPageSize().GetHeight() - word.Height * fontScale - word.Y * fontScale);
                                        }
                                        pdfCanvas.ShowText(word.Text);
                                    }
                                    pdfCanvas.EndText();
                                }
                            }
                        }
                    }
                }
                PdfStream = newPdfStream;

            } catch {
                newPdfStream.Dispose();
                throw;
            }
        }
    }
}
