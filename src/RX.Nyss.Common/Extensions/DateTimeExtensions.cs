using System;

namespace RX.Nyss.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ApplyTimeZone(this DateTime date, TimeZoneInfo timeZone) =>
            TimeZoneInfo.ConvertTimeFromUtc(date, timeZone);
    }
}
