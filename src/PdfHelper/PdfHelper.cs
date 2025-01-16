using System;
using System.IO;
using iText.Kernel.Pdf;

namespace Sitl.Pdf {

    public partial class PdfHelper : IDisposable {
        Stream pdfStream;

        /// <summary>
        /// Initializes a new instance of the Sitl.Pdf.PdfHelper class with 1 or more blank page(s).
        /// </summary>
        public PdfHelper(PageSize pageSize, int nbPages = 1) {
            if (nbPages < 1) throw new ArgumentOutOfRangeException(nameof(nbPages));
            pdfStream = new MemoryStream();
            try {
                using (PdfWriter writer = new PdfWriter(pdfStream)) {
                    writer.SetCloseStream(false);
                    using (PdfDocument doc = new PdfDocument(writer)) {
                        for (int i = 1; i <= nbPages; i++)
                            doc.AddNewPage(new iText.Kernel.Geom.PageSize(pageSize.Width, pageSize.Height));
                    }
                }
            } catch {
                pdfStream.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Initializes a new instance of the Sitl.Pdf.PdfHelper class with a PDF file.
        /// </summary>
        public PdfHelper(string pdfFilePath) 
            : this(File.OpenRead(pdfFilePath)) { 
        }

        /// <summary>
        /// Initializes a new instance of the Sitl.Pdf.PdfHelper class with a PDF document.
        /// </summary>
        public PdfHelper(byte[] pdf)
            : this(new MemoryStream(pdf)) { 
        }

        /// <summary>
        /// Initializes a new instance of the Sitl.Pdf.PdfHelper class with a PDF document.
        /// </summary>
        public PdfHelper(Stream pdf) {
            if (!IsValidPdf(pdf)) throw new ArgumentException("Invalid PDF");
            pdfStream = pdf;
        }

        /// <summary>
        /// Copies a Sitl.Pdf.PdfHelper.
        /// </summary>
        public PdfHelper(PdfHelper copy) {
            copy.PdfStream.CopyTo(pdfStream);
        }

        public Stream PdfStream {
            get {
                if (pdfStream != null) 
                    pdfStream.Position = 0;
                return pdfStream;
            }
            set {
                pdfStream?.Dispose();
                pdfStream = value;
            }
        }

        public byte[] ToByteArray() {
            return pdfStream.ReadAsBytes(true);
        }

        public void Save(string pdfFilePath) {
            using (var fileStream = File.Create(pdfFilePath)) {
                PdfStream.CopyTo(fileStream);
            }
        }

        public double EvaluateSizeOnDiskInBytes() {
            return pdfStream.ReadAsBytes(true).EvaluateSizeOnDiskInBytes();
        }

        public double EvaluateSizeOnDiskInKb() {
            return pdfStream.ReadAsBytes(true).EvaluateSizeOnDiskInKb();
        }

        public double EvaluateSizeOnDiskInMb() {
            return pdfStream.ReadAsBytes(true).EvaluateSizeOnDiskInMb();
        }

        public void Dispose() {
            pdfStream?.Dispose();
            pdfStream = null;
        }

        public static bool IsValidPdf(string pdfFilePath) {
            return IsValidPdf(File.OpenRead(pdfFilePath).ReadAsBytes(closeStream: true));
        }

        public static bool IsValidPdf(Stream stream) {
            if (stream == null) return false;
            stream.Position = 0;
            byte[] buffer = new byte[1024];
            stream.Read(buffer, 0, buffer.Length);
            return IsValidPdf(buffer);
        }

        public static bool IsValidPdf(byte[] bytes) {
            //https://stackoverflow.com/questions/6186980/determine-if-a-byte-is-a-pdf-file#:~:text=J%27ai%20utilis%C3%A9%20cette,%C3%A0%2019%3A15
            if (bytes?.Length < 4) return false;
            var stopBefore = Math.Min(bytes.Length, 1024) - 3;
            for (var i = 0; i < stopBefore; i++)
                if (bytes[i] == '%' && bytes[i + 1] == 'P' && bytes[i + 2] == 'D' && bytes[i + 3] == 'F') return true;
            return false;
        }
    }
}
