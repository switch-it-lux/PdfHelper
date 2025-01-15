using System;
using System.Collections.Generic;
using System.Linq;

namespace Sit.Pdf {

    internal static class DictionaryExtensions {

        public static bool TryGetValue<T>(this IDictionary<string, T> dict, string key, StringComparison keyComparison, out T result) {
            result = default;
            if (dict == null) return false;
            if (!dict.ContainsKey(key)) return false;
            result = dict.FirstOrDefault(x => x.Key.Equals(key, keyComparison)).Value;
            return true;
        }

        public static bool TryGetValue<T>(this IDictionary<string, object> dict, string key, StringComparison keyComparison, out T result) {
            result = default;
            if (dict == null) return false;
            if (!dict.ContainsKey(key)) return false;
            var v = dict.FirstOrDefault(x => x.Key.Equals(key, keyComparison)).Value;
            if (v is T t) {
                result = t;
                return true;
            }
            return false;
        }
    }
}
