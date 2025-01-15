using System;
using System.Drawing;
using System.Drawing.Imaging;
using SkiaSharp;

namespace Sit.Pdf {

    internal static class SkiaExtensions {

        //Inspired from assembly SkiaSharp.Views.Desktop.Common

        public static Bitmap ToBitmap(this SKImage skiaImage) {
            Bitmap bitmap = new Bitmap(skiaImage.Width, skiaImage.Height, PixelFormat.Format32bppPArgb);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            using (SKPixmap pixmap = new SKPixmap(new SKImageInfo(bitmapData.Width, bitmapData.Height), bitmapData.Scan0, bitmapData.Stride)) {
                skiaImage.ReadPixels(pixmap, 0, 0);
            }
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        public static Bitmap ToBitmap(this SKBitmap skiaBitmap) {
            using (SKPixmap pixmap = skiaBitmap.PeekPixels()) {
                using (SKImage skiaImage = SKImage.FromPixels(pixmap)) {
                    if (skiaImage != null) {
                        Bitmap result = skiaImage.ToBitmap();
                        GC.KeepAlive(skiaBitmap);
                        return result;
                    } else {
                        return null;
                    }
                }
            }
        }
    }
}
