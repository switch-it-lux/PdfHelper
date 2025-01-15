using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iText.Kernel.Pdf;

namespace Sit.Pdf {

    public partial class PdfHelper {

        /// <summary>
        /// Extracts the specified page ranges from the PDF.
        /// </summary>
        /// <param name="pageRange">The page range, e.g.: "3-5".</param>
        public byte[] ExtractPageRange(string pageRange) {
            var splitHelper = new PdfSplitterHelper(PdfStream);
            return splitHelper.ExtractPageRange(pageRange);
        }

        /// <summary>
        /// Splits the PDF by page numbers.
        /// </summary>
        /// <param name="pageNumbers">The numbers of pages from which another document is to be started. If the first element is not 1, then 1 is implied (i.e. the first split document will start from page 1 in any case).</param>
        /// <param name="removeFirstPage">True if the first page should be removed.</param>
        public IList<byte[]> SplitByPageNumbers(IEnumerable<int> pageNumbers, bool removeFirstPage = false) {
            if (pageNumbers.Any(x => x < 1 || x > GetNumberOfPages())) throw new Exception("Invalid split page number");
            var splitHelper = new PdfSplitterHelper(PdfStream);
            return splitHelper.SplitByPageNumbers(pageNumbers, removeFirstPage);
        }

        /// <summary>
        /// Splits the PDF by the specified max size (in bytes).
        /// </summary>
        /// <param name="maxBytes">The requested max size (in bytes).</param>
        public IList<byte[]> SplitByMaxSize(long maxBytes) {
            var docs = new List<byte[]>();
            var bounds = FindPageBoundsForSplitByMaxSize(maxBytes);

            int firstPage = 1;
            foreach (var lastPage in bounds) {
                docs.Add(ExtractPageRange($"{firstPage}-{lastPage}"));
                firstPage = lastPage + 1;
            }
            return docs;
        }

        /// <summary>
        /// Finds the page bounds for splitting the PDF by max size. I.e.: the numbers of pages where the document must be cut.
        /// </summary>
        /// <param name="maxBytes">The requested max size (in bytes).</param>
        int[] FindPageBoundsForSplitByMaxSize(long maxBytes) {
            var pages = new List<int>();
            int firstPage = 1;
            int lastPage;
            do {
                lastPage = FindNextPageBelowMaxSize(maxBytes, firstPage);
                if (lastPage != -1) {
                    pages.Add(lastPage);
                    firstPage = lastPage + 1;
                }
            } while (lastPage >= 0);
            return pages.OrderBy(x => x).ToArray();
        }

        int FindNextPageBelowMaxSize(long maxBytes, int startPage = 1) {
            //Binary search
            int lowerBound = startPage;
            int higherBound = GetNumberOfPages();
            int middleBound = higherBound;
            int res = higherBound;
            double bytes = double.MaxValue;
            if (startPage < 1 || startPage > higherBound) return -1;

            while (lowerBound <= higherBound) {
                bytes = ExtractPageRange($"{startPage}-{middleBound}").EvaluateSizeOnDiskInBytes();
                if (bytes > maxBytes) {
                    higherBound = middleBound - 1;
                } else {
                    res = middleBound;
                    lowerBound = middleBound + 1;
                }
                middleBound = lowerBound + (int)Math.Ceiling((higherBound - lowerBound) / 2.0);
            }
            if (bytes > maxBytes) throw new Exception($"Cannot split Pdf into {maxBytes} bytes (at least one page would produce a larger PDF)");
            return res;
        }

        class PdfSplitterHelper : iText.Kernel.Utils.PdfSplitter {
            MemoryStream currentOutputStream;

            public PdfSplitterHelper(Stream stream) 
                : base(new PdfDocument(new PdfReader(stream))) {
            }

            public MemoryStream CurrentMemoryStream => currentOutputStream;

            public IList<byte[]> SplitByPageNumbers(IEnumerable<int> pageNumbers, bool removeFirstPage) {
                var documentReadyListener = new DocumentReadyListener(this, removeFirstPage);
                SplitByPageNumbers(pageNumbers.ToList(), documentReadyListener);
                var res = documentReadyListener.Content;
                if (res.Count == 0) return null;
                return res;
            }

            public byte[] ExtractPageRange(string pageRange) {
                var result = ExtractPageRange(new iText.Kernel.Utils.PageRange(pageRange));
                if (result.GetNumberOfPages() > 0) {
                    result.Close();
                    return CurrentMemoryStream.ToArray();
                }
                return null;
            }

            protected override PdfWriter GetNextPdfWriter(iText.Kernel.Utils.PageRange documentPageRange) {
                currentOutputStream = new MemoryStream();
                return new PdfWriter(currentOutputStream);
            }

            class DocumentReadyListener : IDocumentReadyListener {
                readonly PdfSplitterHelper splitter;
                readonly bool removeFirstPage;

                public DocumentReadyListener(PdfSplitterHelper splitter, bool removeFirstPage) {
                    this.splitter = splitter;
                    this.removeFirstPage = removeFirstPage;
                }

                public IList<byte[]> Content { get; } = new List<byte[]>();

                public void DocumentReady(PdfDocument pdfDocument, iText.Kernel.Utils.PageRange pageRange) {
                    if (removeFirstPage) pdfDocument.RemovePage(1);
                    if (pdfDocument.GetNumberOfPages() > 0) {
                        pdfDocument.Close();
                        Content.Add(splitter.CurrentMemoryStream.ToArray());
                    }
                }
            }
        }
    }
}
