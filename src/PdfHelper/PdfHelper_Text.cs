using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Extgstate;

namespace Sitl.Pdf {

    public partial class PdfHelper {

        /// <summary>
        /// Returns all text of the document using a tolerant extraction strategy.
        /// </summary>
        /// <param name="page">Page number (1-based). -1 for all pages.</param>
        /// <param name="toleranceX">Maximum horizontal distance (points) to consider words in the group of words.</param>
        /// <param name="toleranceY">Maximum vertical distance (points) to consider words on the same line.</param>
        public string GetAllText(int page = -1, float toleranceX = 2f, float toleranceY = 2f) {
            var sb = new StringBuilder();
            using (PdfReader reader = new PdfReader(PdfStream)) {
                using (PdfDocument pdfDoc = new PdfDocument(reader)) {
                    for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++) {
                        if (page <= 0 || page == i) {
                            string text = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), new TolerantTextExtractionStrategy(toleranceX, toleranceY));
                            sb.Append(text).AppendLine().AppendLine();
                        }
                    }
                }
            }
            return sb.ToString();
        }

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

        /// <summary>
        /// Custom strategy that tolerates small vertical and horizontal differences between words.
        /// </summary>
        class TolerantTextExtractionStrategy : ITextExtractionStrategy {
            readonly float toleranceX;
            readonly float toleranceY;
            readonly List<WordChunk> chunks = new List<WordChunk>();

            public TolerantTextExtractionStrategy(float toleranceX = 2f, float toleranceY = 2f) {
                this.toleranceX = toleranceX;
                this.toleranceY = toleranceY;
            }

            public void EventOccurred(IEventData data, EventType type) {
                if (type == EventType.RENDER_TEXT) {
                    var renderInfo = (TextRenderInfo)data;
                    var bottomLeft = renderInfo.GetDescentLine().GetStartPoint();
                    var topRight = renderInfo.GetAscentLine().GetEndPoint();
                    chunks.Add(new WordChunk {
                        Text = renderInfo.GetText(),
                        XStart = bottomLeft.Get(Vector.I1),
                        XEnd = topRight.Get(Vector.I1),
                        Y = bottomLeft.Get(Vector.I2)
                    });
                }
            }

            public ICollection<EventType> GetSupportedEvents() => null;

            public string GetResultantText() {
                // Group chunks by line (Y tolerance)
                var lines = new List<List<WordChunk>>();
                foreach (var chunk in chunks) {
                    bool added = false;
                    foreach (var line in lines) {
                        if (Math.Abs(chunk.Y - line[0].Y) <= toleranceY) {
                            line.Add(chunk);
                            added = true;
                            break;
                        }
                    }
                    if (!added) lines.Add(new List<WordChunk> { chunk });
                }

                // Merge horizontally close words (X tolerance) and generate text
                var sb = new StringBuilder();
                foreach (var line in lines) {
                    var sorted = line.OrderBy(c => c.XStart).ToList();
                    var merged = new List<string>();

                    WordChunk prev = null;
                    foreach (var chunk in sorted) {
                        if (prev == null) {
                            merged.Add(chunk.Text);
                        } else {
                            if (chunk.XStart - prev.XEnd <= toleranceX)
                                merged[merged.Count - 1] += chunk.Text;
                            else
                                merged.Add(chunk.Text);
                        }
                        prev = chunk;
                    }
                    sb.AppendLine(string.Join(" ", merged));
                }
                return sb.ToString();
            }

            class WordChunk {
                public string Text;
                public float XStart;
                public float XEnd;
                public float Y;
            }
        }
    }
}
