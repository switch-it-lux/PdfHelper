using System;
using System.Collections.Generic;
using System.Linq;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace Sit.Pdf {

    public partial class PdfHelper {

        /// <summary>
        /// Checks if the PDF document contains a specific word.
        /// </summary>
        public bool ContainsWord(string word) { 
            return GetWordLocations(word).Count() > 0; 
        }

        /// <summary>
        /// Checks if the PDF document contains specific words.
        /// </summary>
        public bool[] ContainsWords(IEnumerable<string> words) { 
            return ContainsWords(words.ToArray()); 
        }

        /// <summary>
        /// Checks if the PDF document contains specific words.
        /// </summary>
        public bool[] ContainsWords(params string[] words) {
            foreach (var word in words) if (!IsWord(word)) throw new Exception("Invalid word");
            var res = new bool[words.Length];
            using (PdfReader reader = new PdfReader(PdfStream)) {
                using (PdfDocument pdfDoc = new PdfDocument(reader)) {
                    var parser = new PdfDocumentContentParser(pdfDoc);
                    int pageCount = pdfDoc.GetNumberOfPages();
                    for (int i = 1; i <= pageCount && res.Any(x => !x); i++) {
                        var page = pdfDoc.GetPage(i);
                        string content = PdfTextExtractor.GetTextFromPage(page);
                        if (string.IsNullOrEmpty(content)) continue;

                        for (int w = 0; w < words.Length; w++) {
                            if (!res[w]) res[w] = content.IndexOf(words[w], StringComparison.InvariantCultureIgnoreCase) > -1;
                        }
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Gets locations of a specific word in the PDF document.
        /// </summary>
        /// <param name="word">The word to search for.</param>
        /// <param name="ignoreCase"></param>
        /// <param name="searchPage">The page to search, or search all pages if not specified.</param>
        /// <param name="searchAreas">The areas to search, or whole page if not specified</param>
        /// <param name="acceptAreaOverlap">Accepts words that overlaps the area.</param>
        public WordLocation[] GetWordLocations(string word, bool ignoreCase = false, int? searchPage = null, IEnumerable<PdfArea> searchAreas = null, bool acceptAreaOverlap = false) { 
            return GetWordLocations(new[] { word }, ignoreCase, searchPage, searchAreas, acceptAreaOverlap); 
        }

        /// <summary>
        /// Gets locations of specific words in the PDF document.
        /// </summary>
        /// <param name="words">The words to search for.</param>
        /// <param name="ignoreCase"></param>
        /// <param name="searchPage">The page to search, or search all pages if not specified.</param>
        /// <param name="searchAreas">The areas to search, or whole page if not specified</param>
        /// <param name="acceptAreaOverlap">Accepts words that overlaps the area.</param>
        public WordLocation[] GetWordLocations(IEnumerable<string> words, bool ignoreCase = false, int? searchPage = null, IEnumerable<PdfArea> searchAreas = null, bool acceptAreaOverlap = false) { 
            return GetWordLocations(words.ToArray(), ignoreCase, searchPage, searchAreas, acceptAreaOverlap); 
        }

        /// <summary>
        /// Gets locations of specific words in the PDF document.
        /// </summary>
        /// <param name="words">The words to search for.</param>
        public WordLocation[] GetWordLocations(params string[] words) {
            return GetWordLocations(words, false); 
        }

        /// <summary>
        /// Gets locations of specific words in the PDF document.
        /// </summary>
        /// <param name="words">The words to search for.</param>
        /// <param name="ignoreCase"></param>
        /// <param name="searchPage">The page to search, or search all pages if not specified.</param>
        /// <param name="searchAreas">The areas to search, or whole page if not specified</param>
        /// <param name="acceptAreaOverlap">Accepts words that overlaps the area.</param>
        public WordLocation[] GetWordLocations(string[] words, bool ignoreCase, int? searchPage = null, IEnumerable<PdfArea> searchAreas = null, bool acceptAreaOverlap = false) {
            var res = new List<WordLocation>();
            if (words?.Length > 0) {
                using (PdfReader reader = new PdfReader(PdfStream)) {
                    using (PdfDocument pdfDoc = new PdfDocument(reader)) {
                        var parser = new PdfDocumentContentParser(pdfDoc);
                        int pageCount = pdfDoc.GetNumberOfPages();
                        for (int i = 1; i <= pageCount; i++) {
                            if (searchPage == null || searchPage <= 0 || i == searchPage) {
                                var page = pdfDoc.GetPage(i);
                                float pageHeight = page.GetPageSize().GetHeight();
                                res.AddRange(parser
                                    .ProcessContent(i, new ExtractHorizontalWordStrategy(ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))
                                    .SearchWords(words)
                                    .Select(wri => new WordLocation(wri, i, pageHeight))
                                    .Where(x => x.Location.YTop < x.Location.YBottom) //this strange case occured on a hidden text on one doc...
                                    .Where(x => x.Location.IsIncludedInAny(searchAreas, acceptAreaOverlap))
                                );
                            }
                        }
                    }
                }
            }
            return res.OrderBy(l => l.Text).ToArray();
        }

        /// <summary>
        /// Gets locations of all words in the PDF document.
        /// </summary>
        /// <param name="searchPage">The page to search, or search all pages if not specified.</param>
        /// <param name="searchAreas">The areas to search, or whole page if not specified</param>
        /// <param name="acceptAreaOverlap">Accepts words that overlaps the area.</param>
        public WordLocation[] GetAllWordLocations(int? searchPage = null, IEnumerable<PdfArea> searchAreas = null, bool acceptAreaOverlap = false) {
            var res = new List<WordLocation>();
            using (PdfReader reader = new PdfReader(PdfStream)) {
                using (PdfDocument pdfDoc = new PdfDocument(reader)) {
                    var parser = new PdfDocumentContentParser(pdfDoc);
                    int pageCount = pdfDoc.GetNumberOfPages();
                    for (int i = 1; i <= pageCount; i++) {
                        if (searchPage == null || searchPage <= 0 || i == searchPage) {
                            var page = pdfDoc.GetPage(i);
                            float pageHeight = page.GetPageSize().GetHeight();
                            res.AddRange(parser
                                .ProcessContent(i, new ExtractHorizontalWordStrategy())
                                .AllWords()
                                .Select(wri => new WordLocation(wri, i, pageHeight))
                                .Where(x => x.Location.YTop < x.Location.YBottom) //this strange case occured on a hidden text on one doc...
                                .Where(x => x.Location.IsIncludedInAny(searchAreas, acceptAreaOverlap))
                            );
                        }
                    }
                }
            }
            return res.OrderBy(x => x.Location.Page).ThenBy(x => x.Location.YTop).ThenBy(x => x.Location.XLeft).ToArray();
        }

        /// <summary>
        /// Regex search in the PDF document.
        /// </summary>
        /// <param name="regex">The Regex expression to search for.</param>
        /// <param name="stopAfterFirstMatch">Stop after first match or not.</param>
        /// <param name="searchPage">The page to search, or search all pages if not specified.</param>
        /// <param name="searchAreas">The areas to search, or whole page if not specified</param>
        /// <param name="acceptAreaOverlap">Accepts words that overlaps the area.</param>
        public WordLocation[] GetRegexLocations(string regex, bool stopAfterFirstMatch = false, int? searchPage = null, IEnumerable<PdfArea> searchAreas = null, bool acceptAreaOverlap = false) {
            var res = new List<WordLocation>();

            using (PdfReader reader = new PdfReader(PdfStream)) {
                using (PdfDocument pdfDoc = new PdfDocument(reader)) {
                    int pageCount = pdfDoc.GetNumberOfPages();
                    for (int i = 1; i <= pageCount; i++) {
                        if (searchPage == null || searchPage <= 0 || i == searchPage) {
                            var page = pdfDoc.GetPage(i);
                            float pageHeight = page.GetPageSize().GetHeight();
                            var regexStategy = new RegexBasedLocationExtractionStrategy(regex);
                            var parser = new PdfCanvasProcessor(regexStategy);
                            parser.ProcessPageContent(page);
                            res.AddRange(regexStategy
                                .GetResultantLocations()
                                .Reverse()
                                .Select(x => new WordLocation(x, i, pageHeight))
                                .Where(x => x.Location.YTop < x.Location.YBottom) //this strange case occured on a hidden text on one doc...
                                .Where(x => x.Location.IsIncludedInAny(searchAreas, acceptAreaOverlap))
                            );

                            if (res.Count > 0 && stopAfterFirstMatch) return new[] { res[0] };
                        }
                    }
                }
            }
            return res.ToArray();
        }

        //iText extract words strategy class (for horizontal words only)
        internal class ExtractHorizontalWordStrategy : LocationTextExtractionStrategy {
            readonly StringComparison stringComparison;
            readonly List<ExtendedTextChunk> chunks = new List<ExtendedTextChunk>();

            public ExtractHorizontalWordStrategy() : this(StringComparison.InvariantCulture) { }
            public ExtractHorizontalWordStrategy(StringComparison comparison) {
                stringComparison = comparison;
            }

            public override void EventOccurred(IEventData data, EventType type) {
                if (!type.Equals(EventType.RENDER_TEXT)) return;

                TextRenderInfo renderInfo = (TextRenderInfo)data;
                IList<TextRenderInfo> text = renderInfo.GetCharacterRenderInfos();
                var baseLine = renderInfo.GetBaseline();
                var loc = new TextChunkLocationDefaultImp(baseLine.GetStartPoint(), baseLine.GetEndPoint(), renderInfo.GetSingleSpaceWidth());

                ExtendedTextChunk chunk = new ExtendedTextChunk(renderInfo.GetText(), loc, text);
                var magnitude = chunk.GetLocation().OrientationMagnitude();
                if (magnitude <= 5 || magnitude >= -5) chunks.Add(chunk); //take horizontal text only (this simplifies coordinates management)
            }

            //Search occurence(s) of words (located on a single line and rendered horizontally)
            public IEnumerable<WordRenderInfo> SearchWords(params string[] words) {
                if (words == null) words = new string[0];
                foreach (var word in words) if (!IsWord(word)) throw new Exception("Invalid word");

                //assigning a line ids to each chunk
                int lineIdSeq = 0;
                for (int i = 0; i < chunks.Count; i++)
                    chunks[i].LineId = i > 0 && chunks[i - 1].GetLocation().SameLine(chunks[i].GetLocation()) ? lineIdSeq : ++lineIdSeq;

                //searching for words line by line
                var res = new List<WordRenderInfo>();
                foreach (var lineChunks in chunks.GroupBy(c => c.LineId)) {
                    var lineText = string.Concat(lineChunks.SelectMany(l => l.GetText()));
                    var lineCharInfos = lineChunks.SelectMany(c => c.CharInfos);

                    foreach (var word in words) {
                        int i = -1;
                        while (true) {
                            i = lineText.IndexOf(word, i + 1, stringComparison);
                            if (i >= 0)
                                res.Add(new WordRenderInfo(lineCharInfos.Skip(i).Take(word.Length)));
                            else
                                break;
                        }
                    }
                }

                return res;
            }

            public IEnumerable<WordRenderInfo> AllWords() {
                //assigning a line id to each chunk
                int lineIdSeq = 0;
                for (int i = 0; i < chunks.Count; i++)
                    chunks[i].LineId = i > 0 && chunks[i - 1].GetLocation().SameLine(chunks[i].GetLocation()) ? lineIdSeq : ++lineIdSeq;

                //looking for words line by line
                var res = new List<WordRenderInfo>();
                foreach (var lineChunks in chunks.GroupBy(c => c.LineId)) {
                    var chunks = lineChunks.Where(x => !string.IsNullOrWhiteSpace(x.GetText()));

                    //assigning a word id to each chunk
                    int wordIdSeq = 0;
                    for (int i = 0; i < chunks.Count(); i++) {
                        var currenChunk = chunks.ElementAt(i);
                        if (i > 0) {
                            var prevChunk = chunks.ElementAt(i - 1);
                            if (currenChunk.GetText().StartsWith(" ")) wordIdSeq++;
                            else if (prevChunk.GetText().EndsWith(" ")) wordIdSeq++;
                            else if (currenChunk.GetLocation().IsAtWordBoundary(prevChunk.GetLocation())) wordIdSeq++;
                        }
                        currenChunk.WordId = wordIdSeq;
                    }

                    foreach (var wordChunks in chunks.GroupBy(c => c.WordId)) {
                        //split into word when chunks contains space
                        var chars = wordChunks.SelectMany(c => c.CharInfos).ToArray();
                        int i = 0;
                        while (i < chars.Length) {
                            while (i < chars.Length && chars[i].GetText() == " ") i++;
                            if (i == chars.Length) break;
                            int wStart = i;
                            while (i < chars.Length && chars[i].GetText() != " ") i++;
                            int wEnd = i - 1;
                            res.Add(new WordRenderInfo(chars.Skip(wStart).Take(wEnd - wStart + 1)));
                        }
                    }
                }

                return res;
            }

            class ExtendedTextChunk : TextChunk {
                public ExtendedTextChunk(string str, ITextChunkLocation location, IEnumerable<TextRenderInfo> charInfos) : base(str, location) {
                    CharInfos = charInfos;

                    //TextRenderInfo.GetText() seems to require execution immediately -> character are sometimes incorrect without this line...
                    if (charInfos != null) foreach (var charInfo in charInfos) charInfo.GetText();
                }

                public IEnumerable<TextRenderInfo> CharInfos { get; }
                public int LineId { get; set; }
                public int WordId { get; set; }
            }
        }

        public static bool IsWord(string s) {
            return s != null && !s.Contains('\n') && !s.Contains('\r') && !s.Contains('\t') && !s.Contains(' ');
        }
    }
}
