namespace Sitl.Pdf {

    public readonly struct PageSize {
        public float Width { get; }
        public float Height { get; }

        public PageSize(float width, float height) {
            Width = width;
            Height = height;
        }

        internal PageSize(iText.Kernel.Geom.Rectangle r)
            : this(r.GetWidth(), r.GetHeight()) { 
        }

        public PageSize Rotate() {
            return new PageSize(Height, Width);
        }

        public override string ToString() {
            return $"({Width},{Height})";
        }
    }

    public static class PageSizes {
        public static PageSize A0 => new PageSize(iText.Kernel.Geom.PageSize.A0);
        public static PageSize A1 => new PageSize(iText.Kernel.Geom.PageSize.A1);
        public static PageSize A2 => new PageSize(iText.Kernel.Geom.PageSize.A2);
        public static PageSize A3 => new PageSize(iText.Kernel.Geom.PageSize.A3);
        public static PageSize A4 => new PageSize(iText.Kernel.Geom.PageSize.A4);
        public static PageSize A5 => new PageSize(iText.Kernel.Geom.PageSize.A5);
        public static PageSize A6 => new PageSize(iText.Kernel.Geom.PageSize.A6);
        public static PageSize A7 => new PageSize(iText.Kernel.Geom.PageSize.A7);
        public static PageSize A8 => new PageSize(iText.Kernel.Geom.PageSize.A8);
        public static PageSize A9 => new PageSize(iText.Kernel.Geom.PageSize.A9);
        public static PageSize A10 => new PageSize(iText.Kernel.Geom.PageSize.A10);
        public static PageSize B0 => new PageSize(iText.Kernel.Geom.PageSize.B0);
        public static PageSize B1 => new PageSize(iText.Kernel.Geom.PageSize.B1);
        public static PageSize B2 => new PageSize(iText.Kernel.Geom.PageSize.B2);
        public static PageSize B3 => new PageSize(iText.Kernel.Geom.PageSize.B3);
        public static PageSize B4 => new PageSize(iText.Kernel.Geom.PageSize.B4);
        public static PageSize B5 => new PageSize(iText.Kernel.Geom.PageSize.B5);
        public static PageSize B6 => new PageSize(iText.Kernel.Geom.PageSize.B6);
        public static PageSize B7 => new PageSize(iText.Kernel.Geom.PageSize.B7);
        public static PageSize B8 => new PageSize(iText.Kernel.Geom.PageSize.B8);
        public static PageSize B9 => new PageSize(iText.Kernel.Geom.PageSize.B9);
        public static PageSize B10 => new PageSize(iText.Kernel.Geom.PageSize.B10);
        public static PageSize Executive => new PageSize(iText.Kernel.Geom.PageSize.EXECUTIVE);
        public static PageSize Ledger => new PageSize(iText.Kernel.Geom.PageSize.LEDGER);
        public static PageSize Legal => new PageSize(iText.Kernel.Geom.PageSize.LEGAL);
        public static PageSize Letter => new PageSize(iText.Kernel.Geom.PageSize.LETTER);
        public static PageSize Tabloid => new PageSize(iText.Kernel.Geom.PageSize.TABLOID);
    }
}
