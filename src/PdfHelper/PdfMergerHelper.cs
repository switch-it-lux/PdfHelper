using System;
using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;

namespace Sitl.Pdf {

    public class PdfMergerHelper : IDisposable {
        PdfWriter _pdfWriter;
        PdfDocument _pdfDocument;
        PdfMerger _pdfMerger;
        readonly bool _disposeStream;

        public PdfMergerHelper() {
            PdfStream = new MemoryStream();
            _disposeStream = true;
            _pdfWriter = new PdfWriter(PdfStream);
            _pdfWriter.SetCloseStream(false);
            _pdfDocument = new PdfDocument(_pdfWriter);
            _pdfMerger = new PdfMerger(_pdfDocument);
        }

        public PdfMergerHelper(Stream pdfOutputStream) {
            PdfStream = pdfOutputStream ?? throw new ArgumentNullException(nameof(pdfOutputStream));
            _disposeStream = false;
            _pdfWriter = new PdfWriter(PdfStream);
            _pdfWriter.SetCloseStream(false);
            _pdfDocument = new PdfDocument(_pdfWriter);
            _pdfMerger = new PdfMerger(_pdfDocument);
        }

        public Stream PdfStream { get; private set; }
        public int NumberOfPages { get; private set; }

        public void Add(byte[] bytes) {
            using (var ms = new MemoryStream(bytes)) {
                Add(ms);
            }
        }

        public void Add(Stream stream) {
            using (PdfReader reader = new PdfReader(stream)) {
                using (PdfDocument pdfToAdd = new PdfDocument(reader)) {
                    _pdfMerger.Merge(pdfToAdd, 1, pdfToAdd.GetNumberOfPages());
                    NumberOfPages += pdfToAdd.GetNumberOfPages();
                }
            }
        }

        public void AddBlankPage(PageSize pageSize) {
            _pdfDocument.AddNewPage(new iText.Kernel.Geom.PageSize(pageSize.Width, pageSize.Height));
            NumberOfPages++;
        }

        public string DocumentTitle {
            get => _pdfDocument.GetDocumentInfo().GetTitle();
            set => _pdfDocument.GetDocumentInfo().SetTitle(value);
        }

        public string DocumentSubject {
            get => _pdfDocument.GetDocumentInfo().GetSubject();
            set => _pdfDocument.GetDocumentInfo().SetSubject(value);
        }

        public string DocumentAuthor {
            get => _pdfDocument.GetDocumentInfo().GetAuthor();
            set => _pdfDocument.GetDocumentInfo().SetAuthor(value);
        }

        public string DocumentCreator {
            get => _pdfDocument.GetDocumentInfo().GetCreator();
            set => _pdfDocument.GetDocumentInfo().SetCreator(value);
        }

        public string DocumentKeywords {
            get => _pdfDocument.GetDocumentInfo().GetKeywords();
            set => _pdfDocument.GetDocumentInfo().SetKeywords(value);
        }

        public void Save(string pdfFilePath) {
            if (NumberOfPages == 0) throw new InvalidOperationException("Document has no page");
            Close();
            using (var fileStream = File.Create(pdfFilePath)) {
                PdfStream.Position = 0;
                PdfStream.CopyTo(fileStream);
            }
            if (_disposeStream) PdfStream?.Dispose();
        }

        public void Close() {
            if (_pdfMerger != null && NumberOfPages > 0) _pdfMerger.Close(); //iText throws 'Document has no pages' on Close if document has no pages
            _pdfMerger = null;
            if (_pdfDocument != null && NumberOfPages > 0) _pdfDocument.Close(); //iText throws 'Document has no pages' on Close if document has no pages
            _pdfDocument = null;
            _pdfWriter?.Close();
            _pdfWriter = null;
        }

        public void Dispose() {
            Close();
            if (_disposeStream) PdfStream?.Dispose();
        }
    }
}
