using System;
using System.Collections.Generic;
using System.IO;
using iText.Kernel.Pdf;

namespace Sitl.Pdf {

    public partial class PdfHelper {

        public string GetMetadata(string key) {
            if (GetMetadata().TryGetValue(key, StringComparison.InvariantCultureIgnoreCase, out var r)) return r;
            else return null;
        }

        public Dictionary<string, string> GetMetadata() {
            var res = new Dictionary<string, string>();
            using (PdfReader reader = new PdfReader(PdfStream)) {
                using (PdfDocument pdfDoc = new PdfDocument(reader)) {
                    PdfDictionary infoDictionary = pdfDoc.GetTrailer().GetAsDictionary(PdfName.Info);
                    foreach (PdfName key in infoDictionary.KeySet()) {
                        var val = infoDictionary.GetAsString(key);
                        if (val != null)
                            res.Add(key.GetValue(), val.GetValue());
                    }
                }
            }
            return res;
        }

        public void SetMetadata(string key, string value) {
            SetMetadata(new Dictionary<string, string> { { key, value } });
        }

        public void SetMetadata(Dictionary<string, string> metadata) {
            metadata = metadata ?? new Dictionary<string, string>();

            if (metadata.Count > 0) {
                var newPdfStream = new MemoryStream();
                try {
                    using (PdfReader reader = new PdfReader(PdfStream)) {
                        using (PdfWriter writer = new PdfWriter(newPdfStream)) {
                            writer.SetCloseStream(false);
                            using (PdfDocument pdfDoc = new PdfDocument(reader, writer)) {
                                pdfDoc.GetDocumentInfo().SetMoreInfo(metadata);
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
}
