using ZXing;

namespace Sit.Pdf {

    internal static class ZXingExtensions {

        public static string ToFriendlyString(this BarcodeFormat format) {
            var s = format.ToString()
                .ToLower()
                .Replace("_", "");
            return s.Substring(0, 1).ToUpper() + s.Substring(1);
        }

        public static BinaryBitmap ConvertToZXingBitmap(this System.Drawing.Bitmap bitmap) {
            var source = new ZXing.Windows.Compatibility.BitmapLuminanceSource(bitmap);
            var binarizer = new ZXing.Common.HybridBinarizer(source);
            return new BinaryBitmap(binarizer);
        }
    }
}
