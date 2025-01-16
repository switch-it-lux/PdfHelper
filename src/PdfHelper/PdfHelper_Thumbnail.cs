using System;
using System.IO;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;

namespace Sitl.Pdf {

    public partial class PdfHelper {

        /// <summary>
        /// Gets the thumbnail image (if any) for a page of the PDF document.
        /// </summary>
        public byte[] GetThumbnail(int page = 1) {
            using (PdfReader reader = new PdfReader(PdfStream)) {
                using (PdfDocument pdfDoc = new PdfDocument(reader)) {
                    return pdfDoc.GetPage(page)?.GetThumbnailImage()?.GetImageBytes();
                }
            }
        }

        /// <summary>
        /// Sets the thumbnail image for a page of the PDF document.
        /// </summary>
        public void SetThumbnail(Stream thumbnailImage, int page = 1) {
            SetThumbnail(thumbnailImage?.ReadAsBytes(), page);
        }

        /// <summary>
        /// Sets the thumbnail image for a page of the PDF document.
        /// </summary>
        public void SetThumbnail(string thumbnailImagePath, int page = 1) {
            if (string.IsNullOrEmpty(thumbnailImagePath)) {
                SetThumbnail((byte[])null, page);
            } else {
                SetThumbnail(File.ReadAllBytes(thumbnailImagePath), page);
            }
        }

        /// <summary>
        /// Sets the thumbnail image for a page of the PDF document.
        /// </summary>
        public void SetThumbnail(byte[] thumbnailImage, int page = 1) {
            var newPdfStream = new MemoryStream();
            try {
                using (PdfReader reader = new PdfReader(PdfStream)) {
                    using (PdfWriter writer = new PdfWriter(newPdfStream)) {
                        writer.SetCloseStream(false);
                        using (PdfDocument pdfDoc = new PdfDocument(reader, writer)) {
                            if (thumbnailImage == null || thumbnailImage.Length == 0) {
                                //pdf.GetPage(page)?.SetThumbnailImage(null);
                                pdfDoc.GetPage(page)?.Put(PdfName.Thumb, null);
                            } else {
                                PdfImageXObject thObj = new PdfImageXObject(ImageDataFactory.Create(thumbnailImage));
                                pdfDoc.GetPage(page)?.SetThumbnailImage(thObj);
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
        /// Removes the thumbnail image from a page of the PDF document.
        /// </summary>
        public void RemoveThumbnail(int page = 1) {
            SetThumbnail((byte[])null, page);
        }

        /// <summary>
        /// Generates a thumbnail (PNG image) for the requested page of the PDF document.
        /// </summary>
        public byte[] GenerateThumbnail(int page = 1, int? width = null, int? height = null, bool whiteBackground = false, int dpi = 96)
            => GenerateThumbnail(page, width, height, whiteBackground, dpi, out _, out _);

        /// <summary>
        /// Generates a thumbnail (PNG image) for the requested page of the PDF document.
        /// </summary>
        public byte[] GenerateThumbnail(int page, int? width, int? height, out int thumbnailWidth, out int thumbnailHeight)
            => GenerateThumbnail(page, width, height, false, out thumbnailWidth, out thumbnailHeight);

        /// <summary>
        /// Generates a thumbnail (PNG image) for the requested page of the PDF document.
        /// </summary>
        public byte[] GenerateThumbnail(int page, int? width, int? height, bool whiteBackground, out int thumbnailWidth, out int thumbnailHeight)
            => GenerateThumbnail(page, width, height, whiteBackground, 96, out thumbnailWidth, out thumbnailHeight);

        /// <summary>
        /// Generates a thumbnail (PNG image) for the requested page of the PDF document.
        /// </summary>
        public byte[] GenerateThumbnail(int page, int? width, int? height, bool whiteBackground, int dpi, out int thumbnailWidth, out int thumbnailHeight) {
            var docPageCount = GetNumberOfPages();
            if (page <= 0 || page > docPageCount) throw new ArgumentOutOfRangeException(nameof(page));
            var pageSize = GetPageSize(page);

            if ((width == null || width <= 0) && (height == null || height <= 0)) {
                thumbnailWidth = (int)pageSize.Width;
                thumbnailHeight = (int)pageSize.Height;
            } else {
                thumbnailWidth = width != null && width > 0 ? width.Value : -1;
                thumbnailHeight = height != null && height > 0 ? height.Value : -1;
                var ratio = pageSize.Height / pageSize.Width;
                if (thumbnailWidth <= 0) thumbnailWidth = (int)(height / ratio);
                else if (thumbnailHeight <= 0) thumbnailHeight = (int)(width * ratio);
            }

            var renderOptions = new PDFtoImage.RenderOptions(
                Dpi: dpi,
                Width: thumbnailWidth, 
                Height: thumbnailHeight, 
                WithAnnotations: true, 
                BackgroundColor: whiteBackground ? SkiaSharp.SKColors.White : SkiaSharp.SKColors.Transparent
            );
            using (var ms = new MemoryStream()) {
                PDFtoImage.Conversion.SavePng(ms, ToByteArray(), null, page - 1, renderOptions);
                return ms.ToArray();
            }
        }
    }
}
