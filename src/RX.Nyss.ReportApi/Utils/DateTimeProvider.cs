using System;
using System.Linq;

namespace RX.Nyss.ReportApi.Utils
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }

        TimeZoneInfo GetTimeZoneInfo(string timeZoneName);
    }

    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public TimeZoneInfo GetTimeZoneInfo(string timeZoneName) => TimeZoneInfo.GetSystemTimeZones().First(tzi => tzi.Id == timeZoneName ||
                                                                                                                   tzi.DisplayName == timeZoneName ||
                                                                                                                   tzi.StandardName == timeZoneName ||
                                                                                                                   tzi.DaylightName == timeZoneName);
    }
}
