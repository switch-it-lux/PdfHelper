using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Layout;
using iText.Layout.Element;
using Newtonsoft.Json;

namespace Sitl.Pdf {

    public partial class PdfHelper {

        /// <summary>
        /// Adds data dictionary (serialized to a base64 string) to the PDF outside of the visible page frame.
        /// The stamp is delimited by a starting tag and a ending tag.
        /// </summary>
        /// <param name="stampData">The data dictionary.</param>
        /// <param name="stampStartTag">The starting tag.</param>
        /// <param name="stampEndTag">The ending tag.</param>
        /// <param name="addPageNumberToDataStamp">Add Page number to the data dictionary (with the key "Page").</param>
        public void AddDataStamp(Dictionary<string, string> stampData, string stampStartTag, string stampEndTag, bool addPageNumberToDataStamp = false) {
            var newPdfStream = new MemoryStream();
            try {
                using (PdfReader reader = new PdfReader(PdfStream)) {
                    using (PdfWriter writer = new PdfWriter(newPdfStream)) {
                        writer.SetCloseStream(false);
                        using (PdfDocument pdfDoc = new PdfDocument(reader, writer)) {
                            for (int i = 1; i < pdfDoc.GetNumberOfPages() + 1; i++) {
                                var page = pdfDoc.GetPage(i);
                                var pdfCanvas = new PdfCanvas(page);

                                var pageSize = page.GetPageSize();
                                var pageX = pageSize.GetRight();
                                var pageY = pageSize.GetBottom();
                                var canvas = new Canvas(pdfCanvas, pageSize);

                                //Add page number to stamp, replace if already exists
                                if (addPageNumberToDataStamp) {
                                    if (stampData.ContainsKey("Page"))
                                        stampData["Page"] = i.ToString();
                                    else
                                        stampData.Add("Page", i.ToString());
                                }
                                string json = JsonConvert.SerializeObject(stampData);
                                var p = new Paragraph(stampStartTag + Convert.ToBase64String(Encoding.UTF8.GetBytes(json)) + stampEndTag);
                                p.SetFixedPosition(pageX, pageY, json.Length + 4);
                                p.SetFontSize(1).SetFontColor(ColorConstants.WHITE);
                                canvas.Add(p);
                                canvas.Close();
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
        /// Retrieves the data stamps created with method 'AddDataStamps' from the PDF pages. 
        /// The data stamps are delimited by the a starting tag and a ending tag. 
        /// </summary>
        /// <param name="stampStartTag">The starting tag.</param>
        /// <param name="stampEndTag">The ending tag.</param>
        public Dictionary<int, Dictionary<string, string>> GetDataStamps(string stampStartTag, string stampEndTag) {
            using (PdfReader reader = new PdfReader(PdfStream)) {
                using (PdfDocument pdfDoc = new PdfDocument(reader)) {
                    var stamps = new Dictionary<int, Dictionary<string, string>>();

                    for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++) {
                        PdfPage p = pdfDoc.GetPage(page);
                        string content = PdfTextExtractor.GetTextFromPage(p);
                        var match = new Regex(stampStartTag + "(.*)" + stampEndTag).Match(content);
                        Dictionary<string, string> stamp = null;
                        if (match.Success) {
                            stamp = JsonConvert.DeserializeObject<Dictionary<string, string>>(Encoding.UTF8.GetString(Convert.FromBase64String(match.Groups[1].Value)));
                            stamps.Add(page, stamp);
                        }
                    }

                    return stamps;
                }
            }
        }
    }
}
