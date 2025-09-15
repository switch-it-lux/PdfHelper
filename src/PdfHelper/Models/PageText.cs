namespace Sitl.Pdf {

    public class PageText {
        public int Page { get; }
        public string Text { get; }

        internal PageText(int page, string text) {
            Page = page;
            Text = text;
        }
    }
}
