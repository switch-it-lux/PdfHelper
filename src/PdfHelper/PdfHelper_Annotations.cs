using System.Drawing;
using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;

namespace Sit.Pdf {

    public partial class PdfHelper {

        /// <summary>
        /// Checks if the PDF document contains any annotation.
        /// </summary>
        public bool ContainsAnyAnnotations(int page = -1) {
            using (PdfReader reader = new PdfReader(PdfStream)) {
                using (PdfDocument pdfDoc = new PdfDocument(reader)) {
                    for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++) {
                        if (page <= 0 || page == i) {
                            if (pdfDoc.GetPage(i).GetAnnotations().Count > 0) return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Removes all annotations from the PDF document.
        /// </summary>
        public void RemoveAnnotations() {
            var newPdfStream = new MemoryStream();
            try {
                using (PdfReader reader = new PdfReader(PdfStream)) {
                    using (PdfWriter writer = new PdfWriter(newPdfStream)) {
                        writer.SetCloseStream(false);
                        writer.IsFullCompression();
                        using (PdfDocument pdfDoc = new PdfDocument(reader, writer)) {
                            for (int i = 1; i < pdfDoc.GetNumberOfPages() + 1; i++) {
                                var page = pdfDoc.GetPage(i);
                                foreach (var annot in page.GetAnnotations()) {
                                    page.RemoveAnnotation(annot);
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
        /// Highlights (text markup annotation) an area of the PDF document.
        /// </summary>
        public void HighlightArea(PdfArea area, Color highlightColor) {
            var newPdfStream = new MemoryStream();
            try {
                using (PdfReader reader = new PdfReader(PdfStream)) {
                    using (PdfWriter writer = new PdfWriter(newPdfStream)) {
                        writer.SetCloseStream(false);
                        using (PdfDocument pdfDoc = new PdfDocument(reader, writer)) {
                            if (area.Page != null) {
                                var page = pdfDoc.GetPage(area.Page.Value);
                                HighlightRectangle(page, area.ToRectangle(page.GetPageSize().GetHeight()), highlightColor);

                            } else {
                                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++) {
                                    var page = pdfDoc.GetPage(i);
                                    HighlightRectangle(page, area.ToRectangle(page.GetPageSize().GetHeight()), highlightColor);
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

        static void HighlightRectangle(PdfPage page, iText.Kernel.Geom.Rectangle rect, Color color) {
            var quad = new float[] { rect.GetLeft(), rect.GetBottom(), rect.GetRight(), rect.GetBottom(), rect.GetLeft(), rect.GetTop(), rect.GetRight(), rect.GetTop() };
            var annot = new PdfTextMarkupAnnotation(rect, PdfName.Highlight, quad);
            annot.SetColor(new iText.Kernel.Colors.DeviceRgb(color));
            page.AddAnnotation(annot);
        }
    }
}
