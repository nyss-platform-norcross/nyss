using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RX.Nyss.Common.Utils
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }

        int GetEpiWeek(DateTime date);

        EpiDate GetEpiDate(DateTime date);

        IEnumerable<EpiDate> GetEpiWeeksRange(DateTime startDate, DateTime endDate);
    }

    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;

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

        public EpiDate GetEpiDate(DateTime date)
        {
            var epiWeek = GetEpiWeek(date);

            var epiYear = date.Month == 12 && epiWeek == 1
                ? date.Year + 1
                : date.Year;

            return new EpiDate(epiWeek, epiYear);
        }

        public IEnumerable<EpiDate> GetEpiWeeksRange(DateTime startDate, DateTime endDate) =>
            Enumerable
                .Range(0, (endDate.Subtract(startDate).Days / 7) + 1)
                .Select(w => startDate.AddDays(w * 7))
                .Union(new[] { endDate })
                .Select(GetEpiDate)
                .Distinct()
                .ToList();

        public TimeZoneInfo GetTimeZoneInfo(string timeZoneName) =>
            TimeZoneInfo.GetSystemTimeZones().First(tzi => tzi.Id == timeZoneName
                || tzi.DisplayName == timeZoneName
                || tzi.StandardName == timeZoneName
                || tzi.DaylightName == timeZoneName);
    }

    public struct EpiDate
    {
        public EpiDate(int epiWeek, int epiYear)
        {
            EpiWeek = epiWeek;
            EpiYear = epiYear;
        }

        public int EpiWeek { get; }

        public int EpiYear { get; }
    }
}
