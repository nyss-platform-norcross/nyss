using System;

namespace RX.Nyss.Common.Utils
{
    public static class StringExtensions
    {
        public static int? ParseToNullableInt(this string value) => int.TryParse(value, out var outValue)
            ? (int?)outValue
            : null;

        public static string SubstringFromEnd(this string s, int numberOfCharacters)
        {
            var maxNumberOfChars = Math.Min(s.Length, numberOfCharacters);
            return s.Substring(s.Length - maxNumberOfChars, maxNumberOfChars);
        }

        public static string Truncate(this string s, int maxLength)
        {
            if (maxLength <= 0)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(s) || s.Length <= maxLength)
            {
                return s;
            }

            return s.Substring(0, maxLength);
        }

        public static string ToCamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }
            return str;
        }
    }
}
