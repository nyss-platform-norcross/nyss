using System;
using System.Globalization;
using System.Linq;

namespace RX.Nyss.ReportApi.Utils
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }

        TimeZoneInfo GetTimeZoneInfo(string timeZoneName);

        int GetEpiWeek(DateTime date);
    }

    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public TimeZoneInfo GetTimeZoneInfo(string timeZoneName) => TimeZoneInfo.GetSystemTimeZones().First(tzi => tzi.Id == timeZoneName ||
                                                                                                                   tzi.DisplayName == timeZoneName ||
                                                                                                                   tzi.StandardName == timeZoneName ||
                                                                                                                   tzi.DaylightName == timeZoneName);

        public int GetEpiWeek(DateTime date)
        {
            DateTime GetAdjustedDate()
            {
                if (date.Month != 12 || date.Day <= 28)
                {
                    return date;
                }

                var day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
                return day >= DayOfWeek.Sunday && day <= DayOfWeek.Tuesday
                    ? date.AddDays(3)
                    : date;
            }

            var adjustedDate = GetAdjustedDate();
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(adjustedDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);
        }
    }
}
