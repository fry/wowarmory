using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Security.Cryptography;

namespace wowarmory {
    public static class Util {
        public static string ToHexString(this byte[] data) {
            return String.Join("", data.Select(e => e.ToString("2x")).ToArray());
        }

        public static BigInteger ToPositiveBigInt(this byte[] data) {
            var copy = new byte[data.Length + 1];
            copy[data.Length] = 0;
            Array.Copy(data, copy, data.Length);
            return new BigInteger(copy);
        }

        public static byte[] AdjustSize(this byte[] data, int size) {
            var result = new byte[size];
            Array.Copy(data, result, Math.Min(data.Length, size));
            return result;
        }

        public static string Multiply(this string source, int multiplier) {
            StringBuilder sb = new StringBuilder(multiplier * source.Length);
            for (int i = 0; i < multiplier; i++) {
                sb.Append(source);
            }

            return sb.ToString();
        }


        public static string Format(object obj) {
            var builder = new StringBuilder();
            if (obj is System.Collections.IDictionary) {
                var myobj = obj as System.Collections.IDictionary;
                builder.Append("{");
                foreach (System.Collections.DictionaryEntry entry in myobj) {
                    builder.Append(Format(entry.Key) + "=>");
                    builder.Append(Format(entry.Value));
                    builder.Append(", ");
                }

                // remove trailing comma + space
                if (myobj.Count > 0)
                    builder.Remove(builder.Length - 2, 2);

                builder.Append("}");
            } else if (obj is System.Collections.IList) {
                var myobj = obj as System.Collections.IList;
                builder.Append("[");
                foreach (var item in myobj) {
                    builder.Append(Format(item));
                    builder.Append(", ");
                }

                // remove trailing comma + space
                if (myobj.Count > 0)
                    builder.Remove(builder.Length - 2, 2);

                builder.Append("]");
            } else if (obj is string) {
                builder.Append("\"");
                builder.Append(obj.ToString());
                builder.Append("\"");
            } else if (obj is byte) {
                builder.Append("0x");
                builder.Append(((byte)obj).ToString("X2"));
            } else {
                builder.Append(obj.ToString());
            }

            return builder.ToString();
        }

        public static string Format(this Dictionary<string, object> dict) {
            return Format(dict as object);
        }
    }
}
