using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sitl.Pdf {

    public partial class PdfHelper {

        /// <summary>
        /// Searches for barcode locations in the PDF document.
        /// </summary>
        public BarcodeLocation[] GetBarcodeLocations(int? searchPage = null, IEnumerable<PdfArea> searchAreas = null, int? stopAfterXMatch = null, int dpi = 96, CancellationToken cancellationToken = default) {
            if (searchPage <= 0) searchPage = null;

            var res = new List<BarcodeLocation>();
            if (stopAfterXMatch == 0) return res.ToArray();

            //limit search to necessary pages
            int startPage = searchPage != null ? searchPage.Value : searchAreas?.Min(x => x.Page > 0 ? x.Page : null) ?? 1;
            int? nbPages = searchPage != null ? 1 : (int?)null;
            if (nbPages == null && searchAreas != null && searchAreas.All(x => x.Page.HasValue && x.Page.Value > 0))
                nbPages = searchAreas.Max(x => x.Page.Value) - startPage + 1;

            //convert pages to images
            var images = ConvertToBitmapsEnumerator(startPage, nbPages, dpi);

            //search barcodes
            var decoder = GetZXingDecoder();
            while (images.MoveNext()) {
                cancellationToken.ThrowIfCancellationRequested();

                var image = images.Current;
                var result = decoder.decodeMultiple(image.Bitmap.ConvertToZXingBitmap());
                if (result != null) {
                    foreach (var barcode in result) {
                        var b = new BarcodeLocation(barcode, image.Page, image.PageHeight, (float)image.Bitmap.Height / image.PageHeight);
                        if (b.Location.IsIncludedInAny(searchAreas)) res.Add(b);
                    }
                }
                if (stopAfterXMatch != null && stopAfterXMatch == res.Count) return res.Take(stopAfterXMatch.Value).ToArray();
            }

            return res.ToArray();
        }

        static ZXing.Multi.GenericMultipleBarcodeReader GetZXingDecoder(params ZXing.BarcodeFormat[] formats) {
            var reader = new ZXing.MultiFormatReader();
            var hints = new Dictionary<ZXing.DecodeHintType, object>();
            if (formats?.Length > 0) hints.Add(ZXing.DecodeHintType.POSSIBLE_FORMATS, formats);
            //hints.Add(ZXing.DecodeHintType.PURE_BARCODE, true);
            //hints.Add(ZXing.DecodeHintType.TRY_HARDER, true);
            reader.Hints = hints;

            return new ZXing.Multi.GenericMultipleBarcodeReader(reader);

            //var byQuadrantReader = new ZXing.Multi.ByQuadrantReader(reader);
            //return new ZXing.Multi.GenericMultipleBarcodeReader(byQuadrantReader);
        }
    }
}
