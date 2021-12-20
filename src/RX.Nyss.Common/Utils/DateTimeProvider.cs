using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RX.Nyss.Common.Utils
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }

        int GetEpiWeek(DateTime date, DayOfWeek epiWeekStartDay);

        EpiDate GetEpiDate(DateTime date, DayOfWeek epiWeekStartDay);

        IEnumerable<EpiDate> GetEpiDateRange(DateTime startDate, DateTime endDate, DayOfWeek epiWeekStartDay);
        DateTime GetFirstDateOfEpiWeek(int year, int epiWeek, DayOfWeek epiWeekStartDay);
    }

    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public int GetEpiWeek(DateTime date, DayOfWeek epiWeekStartDay)
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

            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(adjustedDate, CalendarWeekRule.FirstFourDayWeek, epiWeekStartDay);
        }

        public EpiDate GetEpiDate(DateTime date, DayOfWeek epiWeekStartDay)
        {
            var epiWeek = GetEpiWeek(date, epiWeekStartDay);

            var epiYear = date.Month == 12 && epiWeek == 1 ? date.Year + 1 :
                date.Month == 1 && epiWeek == 53 ? date.Year - 1 : date.Year;

            return new EpiDate(epiWeek, epiYear);
        }

        public IEnumerable<EpiDate> GetEpiDateRange(DateTime startDate, DateTime endDate, DayOfWeek epiWeekStartDay) =>
            Enumerable
                .Range(0, (endDate.Subtract(startDate).Days / 7) + 1)
                .Select(w => startDate.AddDays(w * 7))
                .Union(new[] { endDate })
                .Select(x => GetEpiDate(x, epiWeekStartDay))
                .Distinct()
                .ToList();

        public DateTime GetFirstDateOfEpiWeek(int year, int epiWeek, DayOfWeek epiWeekStartDay)
        {
            /*
             * EPI week 1 is the first week of the year where the end day of the week falls at least 4 days into the year.
             * When EPI week start day is Sunday, that means the the first week where the Wednesday is in the given year.
             * When EPI week start day is Monday, that means the first week where the Thursday is in the given year.
             */

            var firstDateInEpiYear = GetFirstDateInEpiYear(year, epiWeekStartDay);
            var epiWeeksToAdd = epiWeek - 1;

            return epiWeek == 1
                ? firstDateInEpiYear
                : firstDateInEpiYear.AddDays(epiWeeksToAdd * 7);
        }

        private DateTime GetFirstDateInEpiYear(int year, DayOfWeek epiWeekStartDay)
        {
            var jan1 = new DateTime(year, 1, 1);
            var dayOffset = epiWeekStartDay - jan1.DayOfWeek;
            if (epiWeekStartDay == DayOfWeek.Sunday)
            {
                // if Jan 1st is a Friday or Saturday, the first day in the EPI year is the following Sunday, else it's the preceding Sunday
                return jan1.DayOfWeek < DayOfWeek.Thursday && jan1.DayOfWeek >= DayOfWeek.Sunday
                    ? jan1.AddDays(dayOffset)
                    : jan1.AddDays(7 + dayOffset);
            }

            // If Jan 1st is a Saturday or Sunday, the first day in the EPI year is the following Monday, else it's the preceding Monday
            return jan1.DayOfWeek < DayOfWeek.Friday && jan1.DayOfWeek > DayOfWeek.Sunday
                ? jan1.AddDays(dayOffset)
                : jan1.AddDays(7 + dayOffset);
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
