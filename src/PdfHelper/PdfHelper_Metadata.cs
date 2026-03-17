using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using iText.Kernel.Pdf;

namespace Sitl.Pdf {

    public partial class PdfHelper {

        public object GetMetadata(string key, bool useStringConverter = false) {
            if (GetMetadata(useStringConverter).TryGetValue(key, StringComparison.InvariantCultureIgnoreCase, out var r)) return r;
            else return null;
        }

        public Dictionary<string, object> GetMetadata(bool useStringConverter = false, bool ignoreNullValues = false) {
            var converter = useStringConverter ? (IPdfMetadataConverter)new PdfMetadataStringConverter() : new PdfMetadataConverter();

            var res = new Dictionary<string, object>();
            using (PdfReader reader = new PdfReader(PdfStream)) {
                using (PdfDocument pdfDoc = new PdfDocument(reader)) {
                    var infoDictionary = GetInfoDictionary(pdfDoc);
                    if (infoDictionary == null) return res;
                    foreach (PdfName key in infoDictionary.KeySet()) {
                        var val = converter.ConvertFrom(infoDictionary.Get(key));
                        if (ignoreNullValues && val == null) continue;
                        if (ignoreNullValues && val is string && string.IsNullOrEmpty((string)val)) continue;
                        res.Add(key.GetValue(), converter.ConvertFrom(infoDictionary.Get(key)));
                    }
                }
            }
            return res;
        }

        public void SetMetadata(string key, object value, bool useStringConverter = false) {
            SetMetadata(new Dictionary<string, object> { { key, value } }, useStringConverter);
        }

        public void SetMetadata(Dictionary<string, object> metadata, bool useStringConverter = false, bool removeNullValues = false) {
            if (metadata?.Count > 0) {
                var converter = useStringConverter ? (IPdfMetadataConverter)new PdfMetadataStringConverter() : new PdfMetadataConverter();

                if (metadata.Count > 0) {
                    var newPdfStream = new MemoryStream();
                    try {
                        using (PdfReader reader = new PdfReader(PdfStream)) {
                            using (PdfWriter writer = new PdfWriter(newPdfStream)) {
                                writer.SetCloseStream(false);
                                using (PdfDocument pdfDoc = new PdfDocument(reader, writer)) {
                                    var infoDictionary = GetInfoDictionary(pdfDoc);
                                    foreach (var entry in metadata) {
                                        if (removeNullValues && entry.Value == null)
                                            infoDictionary.Remove(new PdfName(entry.Key));
                                        else
                                            infoDictionary.Put(new PdfName(entry.Key), converter.ConvertTo(entry.Value));
                                    }
                                    infoDictionary.SetModified();
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

        /// <summary>
        /// Gets the PDF info dictionary from the document.
        /// In iText 9, the info dictionary is managed internally by PdfDocumentInfo
        /// and is not accessible via the trailer until the document is closed.
        /// </summary>
        private static PdfDictionary GetInfoDictionary(PdfDocument pdfDoc) {
            var docInfo = pdfDoc.GetDocumentInfo();
            var field = docInfo.GetType().GetField("infoDictionary", BindingFlags.NonPublic | BindingFlags.Instance);
            return (PdfDictionary)field?.GetValue(docInfo);
        }
    }
}
