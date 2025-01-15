using System.IO;
using iText.Kernel.Events;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;

namespace Sitl.Pdf {

    public partial class PdfHelper {

        //See: https://kb.itextpdf.com/home/it7kb/examples/scaling-and-rotating-pages

        public void Scale(float scale) {
            if (scale == 1f) return;

            var newPdfStream = new MemoryStream();
            try {
                using (PdfDocument pdfDocSource = new PdfDocument(new PdfReader(PdfStream))) {
                    using (PdfWriter writer = new PdfWriter(newPdfStream)) {
                        writer.SetCloseStream(false);
                        using (PdfDocument pdfDocTarget = new PdfDocument(writer)) {
                            ScaleEventHandler eventHandler = new ScaleEventHandler(scale);
                            pdfDocTarget.AddEventHandler(PdfDocumentEvent.START_PAGE, eventHandler);

                            int numberOfPages = pdfDocSource.GetNumberOfPages();
                            for (int p = 1; p <= numberOfPages; p++) {
                                eventHandler.SetPageDict(pdfDocSource.GetPage(p).GetPdfObject());

                                // Copy and paste scaled page content as formXObject
                                PdfFormXObject page = pdfDocSource.GetPage(p).CopyAsFormXObject(pdfDocTarget);
                                PdfCanvas canvas = new PdfCanvas(pdfDocTarget.AddNewPage());
                                canvas.AddXObjectWithTransformationMatrix(page, scale, 0f, 0f, scale, 0f, 0f);
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

        class ScaleEventHandler : IEventHandler {
            protected float scale = 1;
            protected PdfDictionary pageDict;

            public ScaleEventHandler(float scale) {
                this.scale = scale;
            }

            public void SetPageDict(PdfDictionary pageDict) {
                this.pageDict = pageDict;
            }

            public void HandleEvent(Event currentEvent) {
                PdfDocumentEvent docEvent = (PdfDocumentEvent)currentEvent;
                PdfPage page = docEvent.GetPage();

                page.Put(PdfName.Rotate, pageDict.GetAsNumber(PdfName.Rotate));

                //The MediaBox value defines the full size of the page.
                Scale(page, pageDict, PdfName.MediaBox, scale);

                //The CropBox value defines the visible size of the page.
                Scale(page, pageDict, PdfName.CropBox, scale);
            }

            protected void Scale(PdfPage destPage, PdfDictionary pageDictSrc, PdfName box, float scale) {
                PdfArray original = pageDictSrc.GetAsArray(box);
                if (original != null) {
                    float width = original.GetAsNumber(2).FloatValue() - original.GetAsNumber(0).FloatValue();
                    float height = original.GetAsNumber(3).FloatValue() - original.GetAsNumber(1).FloatValue();

                    PdfArray result = new PdfArray {
                        new PdfNumber(0),
                        new PdfNumber(0),
                        new PdfNumber(width * scale),
                        new PdfNumber(height * scale)
                    };
                    destPage.Put(box, result);
                }
            }
        }
    }
}
