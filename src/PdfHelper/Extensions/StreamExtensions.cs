using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Sitl.Pdf {

    internal static class StreamExtensions {

        public static string ReadAsString(this Stream stream, bool seekZero = false, bool closeStream = false) {
            if (seekZero) stream.Position = 0;
            using (var sr = new StreamReader(stream)) {
                var res = sr.ReadToEnd();
                if (closeStream) stream.Close();
                return res;
            }
        }

        public static string ReadAsString(this Stream stream, Encoding encoding, bool seekZero = false, bool closeStream = false) {
            if (seekZero) stream.Position = 0;
            using (var sr = new StreamReader(stream, encoding)) {
                var res = sr.ReadToEnd();
                if (closeStream) stream.Close();
                return res;
            }
        }

        public static byte[] ReadAsBytes(this Stream stream, bool seekZero = false, bool closeStream = false) {
            if (seekZero) stream.Position = 0;
            using (var ms = new MemoryStream()) {
                stream.CopyTo(ms);
                var res = ms.ToArray();
                if (closeStream) stream.Close();
                return res;

            }
        }

        public static byte[] ReadAsBytes(this Bitmap bitmap, ImageFormat imageFormat = null) {
            using (var ms = new MemoryStream()) {
                bitmap.Save(ms, imageFormat ?? ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}
