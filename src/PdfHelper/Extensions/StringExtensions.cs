namespace Sitl.Pdf {

    internal static class StringExtensions {

        public static string NullIfEmpty(this string s) {
            if (string.IsNullOrEmpty(s)) return null;
            return s;
        }
    }
}
