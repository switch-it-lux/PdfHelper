using System.IO;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace Sitl.Pdf {

    public partial class PdfHelper {

        public int GetNumberOfPages() {
            using (PdfReader reader = new PdfReader(PdfStream)) {
                using (PdfDocument document = new PdfDocument(reader)) {
                    return document.GetNumberOfPages();
                }
            }
        }

        public PageSize GetPageSize(int page) {
            using (PdfReader reader = new PdfReader(PdfStream)) {
                using (PdfDocument pdfDoc = new PdfDocument(reader)) {
                    return new PageSize(pdfDoc.GetPage(page).GetPageSize());
                }
            }
        }

        public void RemovePage(int pageNumber) {
            var newPdfStream = new MemoryStream();
            try {
                using (PdfReader reader = new PdfReader(PdfStream)) {
                    using (PdfWriter writer = new PdfWriter(newPdfStream)) {
                        writer.SetCloseStream(false);
                        using (PdfDocument pdfDoc = new PdfDocument(reader, writer)) {
                            pdfDoc.RemovePage(pageNumber);
                        }
                    }
                }
                PdfStream = newPdfStream;

            } catch {
                pdfStream.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Adds a blank page at the end of the PDF document or inserts a blank page at the pageIndex position.
        /// </summary>
        public void AddBlankPage(PageSize pageSize, int? pageIndex = null) {
            var newPdfStream = new MemoryStream();
            try {
                using (PdfReader reader = new PdfReader(PdfStream)) {
                    using (PdfWriter writer = new PdfWriter(newPdfStream)) {
                        writer.SetCloseStream(false);
                        using (PdfDocument pdfDoc = new PdfDocument(reader, writer)) {
                            if (pageIndex != null)
                                pdfDoc.AddNewPage(pageIndex.Value, new iText.Kernel.Geom.PageSize(pageSize.Width, pageSize.Height));
                            else
                                pdfDoc.AddNewPage(new iText.Kernel.Geom.PageSize(pageSize.Width, pageSize.Height));
                        }
                    }
                }
                PdfStream = newPdfStream;

            } catch {
                pdfStream.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Adds a image page to the PDF document.
        /// </summary>
        public void AddImagePage(string imageFilePath, float margin = 0f, float scale = 1f, PageSize? pageSize = null) {
            AddImagePage(File.ReadAllBytes(imageFilePath), margin, scale, pageSize);
        }

        /// <summary>
        /// Adds a image page to the PDF document.
        /// </summary>
        public void AddImagePage(Stream image, float margin = 0f, float scale = 1f, PageSize? pageSize = null) {
            AddImagePage(image.ReadAsBytes(true), margin, scale, pageSize);
        }

        /// <summary>
        /// Adds a image page to the PDF document.
        /// </summary>
        public void AddImagePage(byte[] image, float margin = 0f, float scale = 1f, PageSize? pageSize = null) {
            var (pdfImage, pdfPageSize) = PrepareFullPageImage(image, margin, scale, pageSize);
            var newPdfStream = new MemoryStream();
            try {
                using (PdfReader reader = new PdfReader(PdfStream)) {
                    using (PdfWriter writer = new PdfWriter(newPdfStream)) {
                        writer.SetCloseStream(false);
                        using (PdfDocument pdfDoc = new PdfDocument(reader, writer)) {
                            using (Document doc = new Document(pdfDoc, pdfPageSize)) {
                                doc.SetMargins(margin, margin, margin, margin);
                                doc.Add(new AreaBreak(AreaBreakType.LAST_PAGE));
                                doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                                doc.Add(pdfImage);
                            }
                        }
                    }
                }
                PdfStream = newPdfStream;

            } catch {
                pdfStream.Dispose();
                throw;
            }
        }

        static (Image Image, iText.Kernel.Geom.PageSize PageSize) PrepareFullPageImage(byte[] image, float margin = 0f, float scale = 1f, PageSize? pageSize = null) {
            var pdfImgData = ImageDataFactory.Create(image);
            var pdfImage = new Image(pdfImgData);

            //Pdf page size
            iText.Kernel.Geom.PageSize pdfPageSize;
            if (pageSize == null)
                pdfPageSize = new iText.Kernel.Geom.PageSize(pdfImage.GetImageWidth() * scale + 2 * margin, pdfImage.GetImageHeight() * scale + 2 * margin);
            else if (pdfImage.GetImageWidth() > pageSize.Value.Width && pdfImage.GetImageWidth() > pdfImage.GetImageHeight())
                pdfPageSize = new iText.Kernel.Geom.PageSize(pageSize.Value.Height, pageSize.Value.Width);
            else
                pdfPageSize = new iText.Kernel.Geom.PageSize(pageSize.Value.Width, pageSize.Value.Height);

            //Image scaling
            if (pdfImage.GetImageWidth() * scale > pdfPageSize.GetWidth() - 2 * margin || pdfImage.GetImageHeight() * scale > pdfPageSize.GetHeight() - 2 * margin) {
                pdfImage.ScaleToFit(pdfPageSize.GetWidth() - 2 * margin, pdfPageSize.GetHeight() - 2 * margin);
            }

            //Center image horizontally & vertically
            pdfImage.SetFixedPosition((pdfPageSize.GetWidth() - pdfImage.GetImageScaledWidth()) / 2, (pdfPageSize.GetHeight() - pdfImage.GetImageScaledHeight()) / 2);

            return (pdfImage, pdfPageSize);
        }
    }
}
