using System;
using System.Collections.Generic;

namespace Sitl.Pdf {

    public class PdfArea {
        public PdfArea(float xLeft, float yTop, float xRight, float yBottom, int? page = null, bool yStartFromBottom = false) {
            XLeft = xLeft;
            YTop = yTop;
            XRight = xRight;
            YBottom = yBottom;
            YStartFromBottom = yStartFromBottom;
            Page = page;
        }

        public float XLeft { get; set; }
        public float YTop { get; set; }
        public float XRight { get; set; }
        public float YBottom { get; set; }
        public bool YStartFromBottom { get; set; }
        public int? Page { get; set; }

        internal float GetYTopFromTop(float pageHeight) => !YStartFromBottom ? YTop : pageHeight - YTop;
        internal float GetYBottomFromTop(float pageHeight) => !YStartFromBottom ? YBottom : pageHeight - YBottom;
        internal float GetYTopFromBottom(float pageHeight) => YStartFromBottom ? YTop : pageHeight - YTop;
        internal float GetYBottomFromBottom(float pageHeight) => YStartFromBottom ? YBottom : pageHeight - YBottom;

        public PdfArea Copy() => new PdfArea(XLeft, YTop, XRight, YBottom, Page, YStartFromBottom);
        public PdfArea Copy(float pageHeight, bool yFromBottom) {
            var res = Copy();
            if (yFromBottom && !res.YStartFromBottom) {
                res.YTop = GetYTopFromBottom(pageHeight);
                res.YBottom = GetYBottomFromBottom(pageHeight);
                res.YStartFromBottom = true;
            } else if (!yFromBottom && res.YStartFromBottom) {
                res.YTop = GetYTopFromTop(pageHeight);
                res.YBottom = GetYBottomFromTop(pageHeight);
                res.YStartFromBottom = false;
            }
            return res;
        }

        internal iText.Kernel.Geom.Rectangle ToRectangle(float pageHeight) {
            return new iText.Kernel.Geom.Rectangle(XLeft, GetYTopFromBottom(pageHeight) - Math.Abs(YBottom - YTop), XRight - XLeft, Math.Abs(YBottom - YTop));
        }
    }

    public class PdfReadOnlyArea {
        internal PdfReadOnlyArea(int page, float pageHeight, float xLeft, float yTop, float xRight, float yBottom) {
            Page = page;
            PageHeight = pageHeight;
            XLeft = xLeft;
            YTop = yTop;
            XRight = xRight;
            YBottom = yBottom;
        }

        internal PdfReadOnlyArea(int page, float pageHeight, iText.Kernel.Geom.Rectangle pdfRect) {
            Page = page;
            PageHeight = pageHeight;
            XLeft = pdfRect.GetLeft();
            YTop = pageHeight - pdfRect.GetTop();
            XRight = pdfRect.GetRight();
            YBottom = pageHeight - pdfRect.GetBottom();
        }

        internal PdfReadOnlyArea(int page, float pageHeight, ZXing.Result barcode, float barcodeSizeRatio) {
            Page = page;
            PageHeight = pageHeight;
            if (barcode.BarcodeFormat == ZXing.BarcodeFormat.QR_CODE) {
                XLeft = barcode.ResultPoints[0].X / barcodeSizeRatio;
                YTop = pageHeight - barcode.ResultPoints[0].Y / barcodeSizeRatio;
                XRight = barcode.ResultPoints[2].X / barcodeSizeRatio;
                YBottom = pageHeight - barcode.ResultPoints[2].Y / barcodeSizeRatio;

            } else {
                XLeft = barcode.ResultPoints[0].X / barcodeSizeRatio;
                YTop = pageHeight - barcode.ResultPoints[0].Y / barcodeSizeRatio;
                XRight = barcode.ResultPoints[1].X / barcodeSizeRatio;
                YBottom = pageHeight - barcode.ResultPoints[1].Y / barcodeSizeRatio;
            }
        }

        public int Page { get; }
        public float XLeft { get; }
        public float YTop { get; }
        public float XRight { get; }
        public float YBottom { get; }
        public float PageHeight { get; }

        public float XCenter => XLeft + (XRight - XLeft) / 2;
        public float YCenter => YTop + (YBottom - YTop) / 2;

        public float YTopFromBottom => PageHeight - YTop;
        public float YBottomFromBottom => PageHeight - YBottom;
        public float YCenterFromBottom => YBottomFromBottom + (YBottomFromBottom - YTopFromBottom) / 2;

        public PdfArea ToPdfArea(bool yStartFromBottom = false) {
            if (yStartFromBottom)
                return new PdfArea(XLeft, YTopFromBottom, XRight, YBottomFromBottom, Page, true);
            else
                return new PdfArea(XLeft, YTop, XRight, YBottom, Page, false);
        }

        public override string ToString() {
            return $"Pg{Page} {XLeft};{YTop};{XRight};{YBottom}";
        }

        internal bool IsIncludedIn(PdfArea area, bool acceptOverlap = false) {
            if (!acceptOverlap)
                return (area.Page == null || area.Page <= 0 || Page == area.Page.Value)
                    && area.XLeft <= XLeft && area.XRight >= XRight && area.GetYTopFromTop(PageHeight) <= YTop && area.GetYBottomFromTop(PageHeight) >= YBottom;
            else
                return (area.Page == null || area.Page <= 0 || Page == area.Page.Value)
                    && ((area.XLeft <= XLeft && area.XRight >= XLeft) || (area.XLeft <= XRight && area.XRight >= XRight))
                    && ((area.GetYTopFromTop(PageHeight) <= YTop && area.GetYBottomFromTop(PageHeight) >= YTop) || (area.GetYTopFromTop(PageHeight) <= YBottom && area.GetYBottomFromTop(PageHeight) >= YBottom));
        }

        internal bool IsIncludedInAny(IEnumerable<PdfArea> areas, bool acceptOverlap = false) {
            if (areas == null) return true;
            foreach (var area in areas) if (IsIncludedIn(area, acceptOverlap)) return true;
            return false;
        }
    }
}
