namespace Sitl.Pdf {

    public class BitmapPage {
        public int Page { get; }
        public int PageWidth { get; }
        public int PageHeight { get; }
        public int BitmapWidth { get; }
        public int BitmapHeight { get; }
        public System.Drawing.Bitmap Bitmap { get; }

        internal BitmapPage(int page, int pageWidth, int pageHeight, int bitmapWidth, int bitmapHeight, System.Drawing.Bitmap bitmap) {
            Page = page;
            PageWidth = pageWidth;
            PageHeight = pageHeight;
            BitmapWidth = bitmapWidth;
            BitmapHeight = bitmapHeight;
            Bitmap = bitmap;
        }
    }
}
