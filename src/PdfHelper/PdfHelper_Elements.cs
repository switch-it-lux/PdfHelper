using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iText.Barcodes;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Renderer;

namespace Sitl.Pdf {

    public partial class PdfHelper {

        /// <summary>
        /// Adds a basic PDF element to the PDF document.
        /// </summary>
        /// <param name="element">The element to add (text, image, barcode, qrcode).</param>
        public void AddElement(PdfElement element) { 
            AddElements(new[] { element }); 
        }

        /// <summary>
        /// Adds basic PDF elements to the PDF document.
        /// </summary>
        /// <param name="elements">The elements to add (text, image, barcode, qrcode).</param>
        public void AddElements(params PdfElement[] elements) { 
            AddElements((IEnumerable<PdfElement>)elements); 
        }

        /// <summary>
        /// Adds basic PDF elements to the PDF document.
        /// </summary>
        /// <param name="elements">The elements to add (text, image, barcode, qrcode).</param>
        public void AddElements(IEnumerable<PdfElement> elements) {
            if (elements == null || elements.Count() == 0) return;

            int pageCount = GetNumberOfPages();
            var newPdfStream = new MemoryStream();
            try {
                using (PdfReader reader = new PdfReader(PdfStream)) {
                    using (PdfWriter writer = new PdfWriter(newPdfStream)) {
                        writer.SetCloseStream(false);
                        using (var pdfDoc = new PdfDocument(reader, writer)) {
                            Document doc = new Document(pdfDoc);
                            foreach (var gElems in elements.GroupBy(x => x.Page /*new { x.Page, x.Layer }*/)) {
                                if (gElems.Key/*.Page*/ < 1 || gElems.Key/*.Page*/ > pageCount) throw new Exception("Invalid page");

                                //TODO: Implement Underlay (overlay/underlay was not migrated during migration iTextSharp to iText7)
                                //      (Layer was commented in PdfElement.cs)
                                //if (gElems.Key.Layer == PdfElementLayer.Underlay)
                                //    throw new NotImplementedException("Element layer 'Underlay' not implemented");

                                var page = pdfDoc.GetPage(gElems.Key/*.Page*/);

                                foreach (var elem in gElems) {
                                    if (elem is PdfTextElement textElem)
                                        AddTextElement(textElem, doc, page);
                                    else if (elem is PdfImageElement imageElem)
                                        AddImageElement(imageElem, doc, page);
                                    else if (elem is PdfBarcode128Element barcode128Elem)
                                        AddBarcode128Element(barcode128Elem, doc, page);
                                    else if (elem is PdfQrcodeElement qrcodeElem)
                                        AddQrcodeElement(qrcodeElem, doc, page);
                                    else if (elem is PdfDotElement dotElem)
                                        AddDotElement(dotElem, page);
                                    else
                                        throw new NotImplementedException($"PdfElement '{elem.GetType().Name}' not implemented");
                                }
                            }
                            doc.Close();
                        }
                    }
                }
                PdfStream = newPdfStream;

            } catch {
                newPdfStream.Dispose();
                throw;
            }
        }

        void AddTextElement(PdfTextElement text, Document doc, PdfPage page) {
            //TextElement is for single line text only
            string singleLineText = text.Text.Replace(" ", "\u00A0");
            PdfFont font = PdfFontFactory.CreateFont(text.FontName, PdfEncodings.CP1252, PdfFontFactory.EmbeddingStrategy.PREFER_NOT_EMBEDDED);
            Text textElm = new Text(singleLineText);
            textElm.SetNextRenderer(new TextRenderer(textElm));

            float width = font.GetWidth(singleLineText, text.FontSize);
            var dx = text.Anchor == PdfElementAnchor.BaselineLeft ? 0 : text.Anchor == PdfElementAnchor.BaselineCenter ? -width / 2 : -width;

            doc.Add(new Paragraph(textElm)
                .SetFont(font)
                .SetFontColor(new DeviceRgb(text.Color))
                .SetFontSize(text.FontSize)
                .SetRotationAngle(ToRadians(text.Rotation))
                .SetRotationPoint(text.X, page.GetPageSize().GetHeight() - text.Y)
                .SetFixedPosition(page.GetDocument().GetPageNumber(page), text.X + dx, page.GetPageSize().GetHeight() - text.Y, width));
        }

        static void AddImageElement(PdfImageElement image, Document doc, PdfPage page) {
            var imgData = ImageDataFactory.Create(image.Bytes);
            var img = new Image(imgData);

            float imgW = img.GetImageWidth();
            float imgH = img.GetImageHeight();
            float w = imgW;
            float h = imgH;
            if (image.Width > 0 || image.Height > 0) {
                if (image.Width > 0 && image.Height > 0) {
                    w = image.Width;
                    h = image.Height;
                } else if (image.Height > 0) {
                    w = imgW * image.Height / imgH;
                    h = image.Height;
                } else if (image.Width > 0) {
                    w = image.Width;
                    h = imgH * image.Width / imgW;
                }
                img.ScaleAbsolute(w, h);
            }

            img.SetFixedPositionForElement(page, image);
            img.SetRotationAngle(ToRadians(image.Rotation));

            doc.Add(img);
        }

        static void AddBarcode128Element(PdfBarcode128Element barcode, Document doc, PdfPage page) {
            var bc = new Barcode128(doc.GetPdfDocument(), null); // font is null : no text generated below the barcode
            bc.SetCodeType(Barcode128.CODE128);
            bc.SetCode(barcode.Text);
            bc.SetBarHeight(barcode.Height);
            if (barcode.Width > 0) bc.FitWidth(barcode.Width);

            var color = new DeviceRgb(barcode.Color);
            PdfFormXObject barcodeObject = bc.CreateFormXObject(color, color, doc.GetPdfDocument());

            var img = new Image(barcodeObject)
                .SetFixedPositionForElement(page, barcode)
                .SetRotationAngle(ToRadians(barcode.Rotation));

            doc.Add(img);
        }

        static void AddQrcodeElement(PdfQrcodeElement qrcode, Document doc, PdfPage page) {
            //IDictionary<EncodeHintType, object> hints = new Dictionary<EncodeHintType, object>();
            //hints[EncodeHintType.CHARACTER_SET] = "UTF-8";
            BarcodeQRCode qr = new BarcodeQRCode(qrcode.Text/*, hints*/);

            DeviceRgb color = new DeviceRgb(qrcode.Color);
            PdfFormXObject qrcodeObject = qr.CreateFormXObject(color, doc.GetPdfDocument());

            var img = new Image(qrcodeObject)
                .SetWidth(qrcode.Size)
                .SetHeight(qrcode.Size)
                .SetFixedPositionForElement(page, qrcode)
                .SetRotationAngle(ToRadians(qrcode.Rotation));

            doc.Add(img);
        }

        static void AddDotElement(PdfDotElement dot, PdfPage page) {
            DeviceRgb colorRgb = new DeviceRgb(dot.Color);
            float pageHeight = page.GetPageSize().GetHeight();

            PdfCanvas pdfCanvas = new PdfCanvas(page);
            pdfCanvas.SetStrokeColor(colorRgb);
            pdfCanvas.SetFillColor(colorRgb);
            pdfCanvas.SetLineWidth(0);
            pdfCanvas.Circle(dot.X, pageHeight - dot.Y, dot.Radius);
            pdfCanvas.FillStroke();
        }

        static float ToRadians(float degrees) => degrees * (float)(Math.PI / 180.0);
    }
}
