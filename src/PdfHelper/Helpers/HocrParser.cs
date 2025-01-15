using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace Sit.Pdf {

    internal static class HocrParser {

        public static HocrPage[] Parse(string hocrFilePath) {
            using (var stream = File.OpenRead(hocrFilePath)) {
                return Parse(stream);
            }
        }

        public static HocrPage[] Parse(byte[] hocr) {
            using (MemoryStream ms = new MemoryStream(hocr)) {
                return Parse(ms);
            }
        }

        public static HocrPage[] Parse(Stream hocr) {
            var doc = new XmlDocument();
            doc.Load(hocr);
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");

            var res = new List<HocrPage>();
            foreach (XmlNode pageNode in doc.SelectNodes("//xhtml:div[@class='ocr_page']", nsmgr)) {
                var pageNo = GetNodeTitleAttributeAsInt(pageNode, "ppageno") + 1;
                var (_, _, pageWidth, pageHeight) = pageNode.GetNodeTitleAttributeBbox();

                if (pageNo > 0 && pageWidth > 0) {
                    var words = new List<HocrWordLocation>();
                    foreach (XmlNode wordNode in pageNode.SelectNodes(".//xhtml:span[@class='ocrx_word']", nsmgr)) {
                        var text = ReplaceLigatures(wordNode.InnerText);
                        var (x, y, width, height) = wordNode.GetNodeTitleAttributeBbox();

                        var lineNode = wordNode.SelectSingleNode("./parent::xhtml:span[@class='ocr_line']", nsmgr);
                        var angle = lineNode?.GetNodeTitleAttributeAsFloat("textangle") ?? -1;
                        var fontSize = lineNode?.GetNodeTitleAttributeAsFloat("x_fsize") ?? -1;
                        if (fontSize < 0) {
                            //Issue https://github.com/tesseract-ocr/tesseract/issues/3303 (x_size rendered instead of x_fsize)
                            fontSize = lineNode.GetNodeTitleAttributeAsFloat("x_size");
                        }

                        if (x > -1 && !string.IsNullOrEmpty(text)) {
                            words.Add(new HocrWordLocation(text, x, y, width, height, fontSize, angle));
                        }
                    }
                    res.Add(new HocrPage(pageNo, pageWidth, pageHeight, words.ToArray()));
                }
            }

            return res.ToArray();
        }

        static (float x, float y, float width, float height) GetNodeTitleAttributeBbox(this XmlNode node) {
            var coords = node.GetNodeTitleAttributeAsString("bbox")?.Split(' ');
            if (coords != null && coords.Length == 4) {
                var x = float.Parse(coords[0]);
                var y = float.Parse(coords[1]);
                var width = float.Parse(coords[2]) - float.Parse(coords[0]);
                var height = float.Parse(coords[3]) - float.Parse(coords[1]);
                return (x, y, width, height);
            } else {
                return (-1, -1, -1, -1);
            }
        }

        static int GetNodeTitleAttributeAsInt(this XmlNode node, string key) {
            var s = node.GetNodeTitleAttributeAsString(key);
            return s == null ? -1 : int.Parse(s);
        }

        static float GetNodeTitleAttributeAsFloat(this XmlNode node, string key) {
            var s = node.GetNodeTitleAttributeAsString(key);
            return s == null ? -1 : float.Parse(s, CultureInfo.InvariantCulture);
        }

        static string GetNodeTitleAttributeAsString(this XmlNode node, string key) {
            var title = node.Attributes?["title"]?.Value;
            var value = title?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .FirstOrDefault(x => x.StartsWith($"{key} "))?
                .Replace($"{key} ", "")
                .Trim();

            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        static string ReplaceLigatures(string input) {
            return input
                .Replace("ﬀ", "ff")
                .Replace("ﬁ", "fi")
                .Replace("ﬂ", "fl")
                .Replace("ﬀ", "ff")
                .Replace("ﬃ", "ffi")
                .Replace("ﬄ", "ffl")
                .Replace("ﬅ", "ft")
                .Replace("ﬆ", "st");
            //.Replace("—", "-")
            //.Replace("‘", "'")
            //.Replace("’", "'")
            //.Replace("»", ">>");
        }
    }
}
