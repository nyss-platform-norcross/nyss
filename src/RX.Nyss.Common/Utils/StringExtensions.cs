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
            var len = Math.Min(s.Length, numberOfCharacters);
            return s.Substring(len - numberOfCharacters, numberOfCharacters);
        }
    }
}
