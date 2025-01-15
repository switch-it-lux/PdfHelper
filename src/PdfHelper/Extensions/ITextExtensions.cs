using System;
using iText.Kernel.Pdf;
using iText.Layout.Properties;
using iTextElement = iText.Layout.Element;

namespace Sit.Pdf {

    internal static class ITextExtensions {

        public static iTextElement.Paragraph SetRotationPoint(this iTextElement.Paragraph paragraph, float x, float y) {
            paragraph.SetProperty(Property.ROTATION_POINT_X, x);
            paragraph.SetProperty(Property.ROTATION_POINT_Y, y);
            return paragraph;
        }

        public static iTextElement.Image SetFixedPositionForElement(this iTextElement.Image image, PdfPage page, PdfAnchoredElement element) {
            float width = image.GetProperty<UnitValue>(Property.WIDTH)?.GetValue() ?? image.GetImageScaledWidth();
            float height = image.GetProperty<UnitValue>(Property.HEIGHT)?.GetValue() ?? image.GetImageScaledHeight();

            var (newWidth, newHeight) = GetRectangleSizeAfterRotation(width, height, element.Rotation);

            var (dx, dy) = RotateRectangleAnchor(width, height, element.Rotation, element.Anchor);
            dx += element.Anchor == PdfElementAnchor.BaselineLeft ? (newWidth - width) / 2
                : element.Anchor == PdfElementAnchor.BaselineCenter ? newWidth / 2
                : newWidth + (width - newWidth) / 2;
            dy += (newHeight - height) / 2;

            return image.SetFixedPosition(page.GetDocument().GetPageNumber(page), element.X - dx, page.GetPageSize().GetHeight() - element.Y - dy);
        }

        static (float X, float Y) RotateRectangleAnchor(float width, float height, float rotation, PdfElementAnchor anchor) {
            float px = anchor == PdfElementAnchor.BaselineLeft ? -width / 2
                : anchor == PdfElementAnchor.BaselineCenter ? 0
                : width / 2;
            float py = -height / 2;

            //rotation in a cartesian coord system: https://en.wikipedia.org/wiki/Rotation_matrix
            float a = ToRadians(rotation);
            float sin = (float)Math.Sin(a);
            float cos = (float)Math.Cos(a);
            float x = px * cos - py * sin;
            float y = px * sin + py * cos;
            return (x - px, y - py);
        }

        static (float NewWidth, float NewHeight) GetRectangleSizeAfterRotation(float width, float height, float rotation) {
            float a = ToRadians(rotation);
            var newWidth = Math.Abs(height * (float)Math.Sin(a) + width * (float)Math.Cos(a));
            var newHeight = Math.Abs(width * (float)Math.Sin(a) + height * (float)Math.Cos(a));
            return (newWidth, newHeight);
        }

        static float ToRadians(float degrees) => degrees * (float)(Math.PI / 180.0);
    }
}
