namespace Sit.Pdf {

    public class BarcodeLocation {
        public string BarcodeType { get; }
        public string Text { get; }
        public PdfReadOnlyArea Location { get; }

        internal BarcodeLocation(string text, string format, PdfReadOnlyArea location) {
            Text = text;
            BarcodeType = format;
            Location = location;
        }

        internal BarcodeLocation(ZXing.Result barcode, int page, float pageHeight, float barcodeSizeRatio)
            : this(barcode.Text, barcode.BarcodeFormat.ToFriendlyString(), new PdfReadOnlyArea(page, pageHeight, barcode, barcodeSizeRatio)) {
        }
    }
}
