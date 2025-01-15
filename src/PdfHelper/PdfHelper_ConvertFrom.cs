using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using MimeKit;

namespace Sitl.Pdf {

    public enum EmailType { Mime, OutlookMsg }

    public partial class PdfHelper {

        /// <summary>
        /// Initializes a new instance of the Sitl.Pdf.PdfHelper class with one or more images. Each image will induce a new page.
        /// </summary>
        public static PdfHelper FromImages(IEnumerable<string> imageFilePaths, float margin = 0f, float scale = 1f, PageSize? pageSize = null) {
            return FromImages(imageFilePaths.Select(x => File.OpenRead(x).ReadAsBytes(closeStream: true)), margin, scale, pageSize);
        }

        /// <summary>
        /// Initializes a new instance of the Sitl.Pdf.PdfHelper class with one or more images. Each image will induce a new page.
        /// </summary>
        public static PdfHelper FromImages(IEnumerable<Stream> images, float margin = 0f, float scale = 1f, PageSize? pageSize = null) {
            return FromImages(images.Select(x => x.ReadAsBytes()), margin, scale, pageSize);
        }

        /// <summary>
        /// Initializes a new instance of the Sitl.Pdf.PdfHelper class with one or more bitmaps. Each bitmap will induce a new page.
        /// </summary>
        public static PdfHelper FromImages(IEnumerable<Bitmap> bitmaps, float margin = 0f, float scale = 1f, PageSize? pageSize = null) {
            return FromImages(bitmaps.Select(x => x.ReadAsBytes()), margin, scale, pageSize);
        }

        /// <summary>
        /// Initializes a new instance of the Sitl.Pdf.PdfHelper class with one or more images. Each image will induce a new page.
        /// </summary>
        public static PdfHelper FromImages(IEnumerable<byte[]> images, float margin = 0f, float scale = 1f, PageSize? pageSize = null) {
            if (images == null || images.Count() == 0) throw new ArgumentNullException(nameof(images));
            var pdfStream = new MemoryStream();
            using (PdfWriter writer = new PdfWriter(pdfStream)) {
                writer.SetCloseStream(false);
                writer.IsFullCompression();
                using (PdfDocument pdfDoc = new PdfDocument(writer)) {
                    var (pdfImage, pdfPageSize) = PrepareFullPageImage(images.ElementAt(0), margin, scale, pageSize);
                    using (Document doc = new Document(pdfDoc, pdfPageSize)) {
                        doc.SetMargins(margin, margin, margin, margin);
                        for (int i = 0; i < images.Count(); i++) {
                            if (i > 0) {
                                (pdfImage, pdfPageSize) = PrepareFullPageImage(images.ElementAt(i), margin, scale, pageSize);
                                doc.Add(new AreaBreak(pdfPageSize));
                            }
                            doc.Add(pdfImage);
                        }
                    }
                }
            }
            return new PdfHelper(pdfStream);
        }

        /// <summary>
        /// Initializes a new instance of the Sitl.Pdf.PdfHelper class from an HTML string.
        /// </summary>
        public static PdfHelper FromHtml(Stream html, PageSize pageSize) {
            return FromHtml(html.ReadAsString(), pageSize);
        }

        /// <summary>
        /// Initializes a new instance of the Sitl.Pdf.PdfHelper class from an HTML string.
        /// </summary>
        public static PdfHelper FromHtml(string html, PageSize pageSize) {
            var pdfStream = new MemoryStream();
            using (var writer = new PdfWriter(pdfStream)) {
                writer.SetCloseStream(false);
                using (var pdfDoc = new PdfDocument(writer)) {
                    pdfDoc.SetDefaultPageSize(new iText.Kernel.Geom.PageSize(pageSize.Width, pageSize.Height));
                    var options = new ConverterProperties();
                    HtmlConverter.ConvertToPdf(html, pdfDoc, options);
                }
            }
            return new PdfHelper(pdfStream);
        }

        /// <summary>
        /// Initializes a new instance of the Sitl.Pdf.PdfHelper class with an email message.
        /// </summary>
        public static async Task<PdfHelper> FromEmailAsync(string emailFilePath, EmailType emailType, PageSize pageSize, bool includeAttachments = false) {
            using (var ms = File.OpenRead(emailFilePath)) {
                return await FromEmailAsync(ms, emailType, pageSize, includeAttachments);
            }
        }

        /// <summary>
        /// Initializes a new instance of the Sitl.Pdf.PdfHelper class with an email message.
        /// </summary>
        public static async Task<PdfHelper> FromEmailAsync(byte[] email, EmailType emailType, PageSize pageSize, bool includeAttachments = false) {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var ms = new MemoryStream(email)) {
                return await FromEmailAsync(ms, emailType, pageSize, includeAttachments);
            }
        }

        /// <summary>
        /// Initializes a new instance of the Sitl.Pdf.PdfHelper class with an email message.
        /// </summary>
        public static async Task<PdfHelper> FromEmailAsync(Stream email, EmailType emailType, PageSize pageSize, bool includeAttachments = false) {
            MimeMessage mimeMsg;
            if (emailType == EmailType.OutlookMsg)
                mimeMsg = await MimeMessageHelper.LoadOutlookMsgAsync(email, includeAttachments);
            else
                mimeMsg = await MimeMessage.LoadAsync(email);

            var pdfStream = await MimeMessageHelper.ConvertToPdfAsync(mimeMsg, pageSize, includeAttachments);
            return new PdfHelper(pdfStream);
        }
    }
}
