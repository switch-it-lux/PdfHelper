using System;
using System.Collections.Generic;

namespace Sit.Pdf {

    public partial class PdfHelper {

        /// <summary>
        /// Converts the PDF pages to bitmaps.
        /// </summary>
        public BitmapPage[] ConvertToBitmaps(int startPage = 1, int? nbPages = null, int dpi = 96) {
            var res = new List<BitmapPage>();
            var e = ConvertToBitmapsEnumerator(startPage, nbPages, dpi);
            while (e.MoveNext()) res.Add(e.Current);
            return res.ToArray();
        }

        /// <summary>
        /// Returns an enumerator over PDF pages converted to bitmaps.
        /// </summary>
        public IEnumerator<BitmapPage> ConvertToBitmapsEnumerator(int startPage = 1, int? nbPages = null, int dpi = 96, bool whiteBackground = true) {
            if (dpi <= 0 || dpi > 2000) throw new ArgumentOutOfRangeException(nameof(dpi));

            var docNbPages = GetNumberOfPages();
            if (nbPages == null) nbPages = docNbPages - (startPage - 1);
            if (startPage > docNbPages) throw new ArgumentOutOfRangeException(nameof(startPage));
            if (startPage - 1 + nbPages.Value > docNbPages) new ArgumentOutOfRangeException(nameof(nbPages));

            for (int p = startPage; p < startPage + nbPages.Value; p++) {
                var pageSize = GetPageSize(p);
                var renderOptions = new PDFtoImage.RenderOptions(
                    Dpi: dpi,
                    BackgroundColor: whiteBackground ? SkiaSharp.SKColors.White : SkiaSharp.SKColors.Transparent
                );
                using (var img = PDFtoImage.Conversion.ToImage(ToByteArray(), null, p - 1, renderOptions)) {
                    yield return new BitmapPage(p, (int)pageSize.Width, (int)pageSize.Height, img.Width, img.Height, img.ToBitmap());
                }
            }
        }
    }
}
