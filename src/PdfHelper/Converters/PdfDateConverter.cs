//using System;
//using iText.Kernel.Pdf;

//namespace Sit.Pdf {

//    internal static class PdfDateConverter {
        
//        public static bool TryParse(string s, out DateTime result) {
//            result = DateTime.MinValue;
//            if (s == null || !s.StartsWith("D:")) return false;
//            try {
//                result = PdfDate.Decode(s);
//                return true;
//            } catch {
//                return false;
//            }
//        }
//    }
//}
