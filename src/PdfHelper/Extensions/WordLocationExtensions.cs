using System.Collections.Generic;
using System.Linq;

namespace Sitl.Pdf {

    public static class WordLocationExtensions {

        public static string GetText(this IEnumerable<WordLocation> words, bool trim = false) {
            if (words?.Count() > 0) {
                return string.Join(" ", words.Select(a => trim ? a.Text.Trim() : a.Text));
            } else return null;
        }
    }
}
