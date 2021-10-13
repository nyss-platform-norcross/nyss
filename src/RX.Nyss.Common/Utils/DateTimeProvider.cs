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

        IEnumerable<EpiDate> GetEpiDateRange(DateTime startDate, DateTime endDate);
        DateTime GetFirstDateOfEpiWeek(int year, int epiWeek);
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
                : date.Month == 1 && epiWeek == 53
                    ? date.Year - 1
                    : date.Year;

            return new EpiDate(epiWeek, epiYear);
        }

        public IEnumerable<EpiDate> GetEpiDateRange(DateTime startDate, DateTime endDate) =>
            Enumerable
                .Range(0, (endDate.Subtract(startDate).Days / 7) + 1)
                .Select(w => startDate.AddDays(w * 7))
                .Union(new[] { endDate })
                .Select(GetEpiDate)
                .Distinct()
                .ToList();

        public DateTime GetFirstDateOfEpiWeek(int year, int epiWeek)
        {
            var jan1 = new DateTime(year, 1, 1);
            var dayOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            var firstThursdayInYear = jan1.AddDays(dayOffset);
            var firstWeek = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(firstThursdayInYear, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);
            var epiWeekNumber = epiWeek;

            if (firstWeek == 1)
            {
                epiWeekNumber -= 1;
            }

            return firstThursdayInYear.AddDays((epiWeekNumber * 7) - 4);
        }
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
