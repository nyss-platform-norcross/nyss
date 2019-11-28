namespace RX.Nyss.ReportApi.Utils
{
    public static class StringExtensions
    {
        public static int? ParseToNullableInt(this string value) => int.TryParse(value, out var outValue) ? (int?)outValue : null;
    }
}
