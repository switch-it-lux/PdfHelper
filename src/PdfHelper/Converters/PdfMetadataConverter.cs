//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using iText.Kernel.Pdf;

//namespace Sit.Pdf {

//    internal interface IPdfMetadataConverter {
//        object ConvertFrom(PdfObject value);
//        object ConvertFrom(PdfObject value, CultureInfo culture);
//        PdfObject ConvertTo(object value);
//        PdfObject ConvertTo(object value, CultureInfo culture);
//    }

//    /// <summary>
//    /// Converts metadata PdfObject to c# object
//    /// </summary>
//    internal class PdfMetadataConverter : IPdfMetadataConverter {

//        public object ConvertFrom(PdfObject value) => ConvertFrom(value, CultureInfo.CurrentCulture);
//        public object ConvertFrom(PdfObject value, CultureInfo culture) {
//            if (value == null || value.GetObjectType() == PdfObject.NULL) {
//                return null;

//            } else if (value.GetObjectType() == PdfObject.ARRAY) {
//                var a = new List<object>();
//                for (int i = 0; i < ((PdfArray)value).Size(); i++) a.Add(ConvertFrom(((PdfArray)value).Get(i)));
//                return a.ToArray();

//            } else if (value.GetObjectType() == PdfObject.NUMBER) {
//                return (value as PdfNumber).GetValue();

//            } else if (value.GetObjectType() == PdfObject.BOOLEAN) {
//                return (value as PdfBoolean).GetValue();

//            } else if (value.GetObjectType() == PdfObject.STRING) {
//                var s = (value as PdfString).GetValue();
//                if (PdfDateConverter.TryParse(s, out var d)) return d;
//                else return s;

//            } else {
//                return null;
//            }
//        }

//        public PdfObject ConvertTo(object value) => ConvertTo(value, CultureInfo.CurrentCulture);
//        public PdfObject ConvertTo(object value, CultureInfo culture) {
//            if (value == null) {
//                return new PdfNull();

//            } else if (value is Array array) {
//                var a = new PdfArray();
//                for (int i = 0; i < array.Length; i++)
//                    a.Add(ConvertTo(array.GetValue(i), culture));
//                return a;

//            } else if (value is double || value is float || value is int || value is long || value is uint || value is ulong) {
//                return new PdfNumber(Convert.ToDouble(value));

//            } else if (value is bool b) {
//                return new PdfBoolean(b);

//            } else if (value is DateTime d) {
//                return new PdfDate(d).GetPdfObject();

//            } else if (value is string s) {
//                return new PdfString(s);

//            } else {
//                return new PdfNull();
//            }
//        }
//    }

//    /// <summary>
//    /// Converts metadata PdfObject to c# string
//    /// </summary>
//    internal class PdfMetadataStringConverter : IPdfMetadataConverter {

//        public object ConvertFrom(PdfObject value) => ConvertFrom(value, CultureInfo.CurrentCulture);
//        public object ConvertFrom(PdfObject value, CultureInfo culture) {
//            if (value.GetObjectType() == PdfObject.NULL) {
//                return null;

//            } else if (value.GetObjectType() == PdfObject.ARRAY) {
//                var a = new List<string>();
//                for (int i = 0; i < ((PdfArray)value).Size(); i++)
//                    a.Add((string)ConvertFrom(((PdfArray)value).Get(i), culture));
//                return string.Join(" & ", a);

//            } else if (value.GetObjectType() == PdfObject.NUMBER) {
//                return Convert.ToString(((PdfNumber)value).GetValue(), culture);

//            } else if (value.GetObjectType() == PdfObject.BOOLEAN) {
//                return Convert.ToString(((PdfBoolean)value).GetValue(), culture);

//            } else if (value.GetObjectType() == PdfObject.STRING) {
//                var s = (value as PdfString).GetValue();
//                if (PdfDateConverter.TryParse(s, out var d)) return d.ToString("dd/MM/yyyy HH:mm:ss");
//                else return s;

//            } else {
//                return null;
//            }
//        }

//        public PdfObject ConvertTo(object value) => ConvertTo(value, CultureInfo.CurrentCulture);
//        public PdfObject ConvertTo(object value, CultureInfo culture) {
//            if (value == null) {
//                return new PdfNull();

//            } else if (value is Array array) {
//                var a = new List<PdfString>();
//                for (int i = 0; i < array.Length; i++)
//                    a.Add((PdfString)ConvertTo(array.GetValue(i), culture));
//                return new PdfString(string.Join(" & ", a.Select(x => x.GetValue())));

//            } else if (value is double || value is float || value is int || value is long || value is uint || value is ulong) {
//                return new PdfString(Convert.ToString(value, culture));

//            } else if (value is bool) {
//                return new PdfString(Convert.ToString(value, culture));

//            } else if (value is DateTime d) {
//                //return new PdfDate((DateTime)value).GetPdfObject();
//                return new PdfString(d.ToString("dd/MM/yyyy HH:mm:ss"));

//            } else {
//                return new PdfString((string)value);
//            }
//        }
//    }
//}
