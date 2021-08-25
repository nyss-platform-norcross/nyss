using System.Collections.Generic;
using System.Linq;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.DataCollectors.Dto;

namespace RX.Nyss.Web.Features.Common.Extensions
{
    public static class DataCollectorFilters
    {
        public static IEnumerable<DataCollectorPerformance> FilterByStatusForEpiWeeks(this IEnumerable<DataCollectorPerformance> dataCollectors, IEnumerable<PerformanceStatusFilterDto> filters) =>
            filters.Aggregate(dataCollectors, (current, weekFilter) => current.FilterByStatusForEpiWeek(weekFilter));

        private static IEnumerable<DataCollectorPerformance>
            FilterByStatusForEpiWeek(this IEnumerable<DataCollectorPerformance> dataCollectors, PerformanceStatusFilterDto filter) =>
            dataCollectors?.Where(dc => dc.PerformanceInEpiWeeks.Any(p => p.EpiWeek == filter.EpiWeek && p.ReportingStatus == ReportingStatus.ReportingCorrectly && filter.ReportingCorrectly)
                || dc.PerformanceInEpiWeeks.Any(p => p.EpiWeek == filter.EpiWeek && p.ReportingStatus == ReportingStatus.ReportingWithErrors && filter.ReportingWithErrors)
                || dc.PerformanceInEpiWeeks.Any(p => p.EpiWeek == filter.EpiWeek && p.ReportingStatus == ReportingStatus.NotReporting && filter.NotReporting));
    }
}
