using System.IO;
using iText.Kernel.Pdf;

namespace Sit.Pdf {

    public partial class PdfHelper {

        /// <summary>
        /// Append a PDF document to the PDF document.
        /// </summary>
        public void AppendPdf(string pdfFilePath) {
            using (var stream = File.OpenRead(pdfFilePath)) {
                AppendPdf(stream);
            }
        }

        /// <summary>
        /// Append a PDF document to the PDF document.
        /// </summary>
        public void AppendPdf(byte[] pdf) {
            using (var stream = new MemoryStream(pdf)) {
                AppendPdf(stream);
            }
        }

        /// <summary>
        /// Append a PDF document to the PDF document.
        /// </summary>
        public void AppendPdf(Stream pdf) {
            pdf.Position = 0;
            var pdfDocToAdd = new PdfDocument(new PdfReader(pdf));
            try {
                var newPdfStream = new MemoryStream();
                try {
                    using (PdfReader reader = new PdfReader(PdfStream)) {
                        using (PdfWriter writer = new PdfWriter(newPdfStream)) {
                            writer.SetCloseStream(false);
                            writer.IsFullCompression();
                            using (PdfDocument pdfDoc = new PdfDocument(reader, writer)) {
                                var pdfMerger = new iText.Kernel.Utils.PdfMerger(pdfDoc);
                                pdfMerger.Merge(pdfDocToAdd, 1, pdfDocToAdd.GetNumberOfPages());
                            }
                        }
                    }
                    PdfStream = newPdfStream;

                } catch {
                    pdfStream.Dispose();
                    throw;
                }

            } finally {
                var pdfToAddReader = pdfDocToAdd?.GetReader();
                pdfDocToAdd?.Close();
                pdfToAddReader?.Close();
            }
        }
    }
}
