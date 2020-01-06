using System;
using System.Globalization;

namespace RX.Nyss.Web.Utils
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }

        int GetEpiWeek(DateTime date);

        bool IsFirstWeekOfNextYear(DateTime date);
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

        public bool IsFirstWeekOfNextYear(DateTime date)
        {
            var epiWeek = GetEpiWeek(date);

            return epiWeek == 1 && date.Month == 12;
        }
    }
}
