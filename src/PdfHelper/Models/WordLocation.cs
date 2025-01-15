using System.Collections.Generic;
using System.Linq;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace Sitl.Pdf {

    public class WordLocation {
        public string Text { get; }
        public PdfReadOnlyArea Location { get; }

        internal WordLocation(string text, int page, float pageHeight, Rectangle pdfRect) {
            Text = text;
            Location = new PdfReadOnlyArea(page, pageHeight, pdfRect);
        }

        internal WordLocation(string text, PdfReadOnlyArea location) {
            Text = text;
            Location = location;
        }

        internal WordLocation(IPdfTextLocation loc, int page, float pageHeight) {
            Text = loc.GetText();
            Location = new PdfReadOnlyArea(
                page,
                pageHeight,
                loc.GetRectangle().GetLeft(),
                pageHeight - loc.GetRectangle().GetTop(),
                loc.GetRectangle().GetRight(),
                pageHeight - loc.GetRectangle().GetBottom()
            );
        }

        internal WordLocation(WordRenderInfo wri, int page, float pageHeight) {
            Text = wri.Text;
            Location = new PdfReadOnlyArea(
                page,
                pageHeight,
                wri.TopLeft.Get(Vector.I1),
                pageHeight - wri.TopLeft.Get(Vector.I2),
                wri.BottomRight.Get(Vector.I1),
                pageHeight - wri.BottomRight.Get(Vector.I2)
            );
        }
    }

    internal class WordRenderInfo {
        public string Text { get; }
        public Vector TopLeft { get; }
        public Vector BottomRight { get; }

        internal WordRenderInfo(IEnumerable<TextRenderInfo> charInfos) {
            Text = string.Concat(charInfos.Select(c => c.GetText()));
            TopLeft = charInfos.First().GetAscentLine().GetStartPoint();
            BottomRight = charInfos.Last().GetDescentLine().GetEndPoint();
        }
    }
}
