using System;
using System.Collections.Generic;
using System.Linq;

namespace RX.Nyss.Common.Extensions;

public static class DateTimeExtensions
{
    public static IEnumerable<DateTime> GetDaysRange(this DateTime startDate, DateTime endDate) =>
        Enumerable
            .Range(0, endDate.Subtract(startDate).Days)
            .Select(i => startDate.AddDays(i));
}