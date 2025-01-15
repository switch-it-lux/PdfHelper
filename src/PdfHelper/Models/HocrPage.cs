namespace Sit.Pdf {

    internal class HocrPage {
        public int Page { get; }
        public float Width { get; }
        public float Height { get; }
        public HocrWordLocation[] WordLocations { get; }

        internal HocrPage(int page, float width, float height, HocrWordLocation[] wordLocations) {
            Page = page;
            Width = width;
            Height = height;
            WordLocations = wordLocations;
        }
    }

    internal class HocrWordLocation {
        public string Text { get; }
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }
        public float FontSize { get; }
        public float TextAngle { get; }

        internal HocrWordLocation(string text, float x, float y, float width, float height, float fontSize, float textAngle) {
            Text = text;
            X = x;
            Y = y;
            Width = width; 
            Height = height;
            FontSize = fontSize;
            TextAngle = textAngle;
        }
    }
}
