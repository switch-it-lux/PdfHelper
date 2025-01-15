namespace Sit.Pdf {

    public enum PdfElementAnchor { BaselineLeft, BaselineCenter, BaselineRight }
    //public enum PdfElementLayer { Overlay, Underlay }

    public abstract class PdfElement {
        public int Page { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Rotation { get; set; } = 0;
        //public PdfElementLayer Layer { get; set; } = PdfElementLayer.Overlay;

        protected PdfElement() { }
        protected PdfElement(PdfElement copy) {
            Page = copy.Page;
            X = copy.X;
            Y = copy.Y;
            Rotation = copy.Rotation;
            //Layer = copy.Layer;
        }
    }

    public abstract class PdfAnchoredElement : PdfElement {
        public PdfElementAnchor Anchor { get; set; } = PdfElementAnchor.BaselineLeft;

        protected PdfAnchoredElement() { }
        protected PdfAnchoredElement(PdfAnchoredElement copy) : base(copy) {
            Anchor = copy.Anchor;
        }
    }

    public class PdfTextElement : PdfAnchoredElement {
        public string Text { get; set; }
        public float FontSize { get; set; } = 10;
        public string FontName { get; set; } = "Helvetica";
        public System.Drawing.Color Color { get; set; } = default;

        public PdfTextElement() { }
        public PdfTextElement(PdfTextElement copy) : base(copy) {
            Text = copy.Text;
            FontSize = copy.FontSize;
            FontName = copy.FontName;
            Color = copy.Color;
        }
    }

    public class PdfImageElement : PdfAnchoredElement {
        public byte[] Bytes { get; set; }
        public float Height { get; set; } = -1;
        public float Width { get; set; } = -1;

        public PdfImageElement() { }
        public PdfImageElement(PdfImageElement copy) : base(copy) {
            Bytes = copy.Bytes;
            Height = copy.Height;
            Width = copy.Width;
        }
    }

    public class PdfBarcode128Element : PdfAnchoredElement {
        public string Text { get; set; }
        public float Height { get; set; } = 24;
        public float Width { get; set; } = -1;
        public System.Drawing.Color Color { get; set; } = default;

        public PdfBarcode128Element() { }
        public PdfBarcode128Element(PdfBarcode128Element copy) : base(copy) {
            Text = copy.Text;
            Height = copy.Height;
            Width = copy.Width;
            Color = copy.Color;
        }
    }

    public class PdfQrcodeElement : PdfAnchoredElement {
        public string Text { get; set; }
        public int Size { get; set; } = 50;
        public System.Drawing.Color Color { get; set; } = default;

        public PdfQrcodeElement() { }
        public PdfQrcodeElement(PdfQrcodeElement copy) : base(copy) {
            Text = copy.Text;
            Size = copy.Size;
            Color = copy.Color;
        }
    }

    public class PdfDotElement : PdfElement {
        public float Radius { get; set; } = 1;
        public System.Drawing.Color Color { get; set; } = default;

        public PdfDotElement() { }
        public PdfDotElement(PdfDotElement copy) : base(copy) {
            Radius = copy.Radius;
            Color = copy.Color;
        }
    }
}
