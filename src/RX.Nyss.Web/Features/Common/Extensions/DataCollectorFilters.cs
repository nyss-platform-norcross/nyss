using System.Collections.Generic;
using System.Linq;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.DataCollectors.Dto;

namespace RX.Nyss.Web.Features.Common.Extensions
{
    public static class DataCollectorFilters
    {
        public static IEnumerable<DataCollectorPerformance>
            FilterByStatusLastWeek(this IEnumerable<DataCollectorPerformance> dataCollectors, PerformanceStatusFilterDto filters) =>
            dataCollectors?.Where(dc => (dc.StatusLastWeek == ReportingStatus.ReportingCorrectly && filters.ReportingCorrectly)
                || (dc.StatusLastWeek == ReportingStatus.ReportingWithErrors && filters.ReportingWithErrors)
                || (dc.StatusLastWeek == ReportingStatus.NotReporting && filters.NotReporting));

        public static IEnumerable<DataCollectorPerformance> FilterByStatusTwoWeeksAgo(this IEnumerable<DataCollectorPerformance> dataCollectors,
            PerformanceStatusFilterDto filters) =>
            dataCollectors?.Where(dc => (dc.StatusTwoWeeksAgo == ReportingStatus.ReportingCorrectly && filters.ReportingCorrectly)
                || (dc.StatusTwoWeeksAgo == ReportingStatus.ReportingWithErrors && filters.ReportingWithErrors)
                || (dc.StatusTwoWeeksAgo == ReportingStatus.NotReporting && filters.NotReporting));

        public static IEnumerable<DataCollectorPerformance> FilterByStatusThreeWeeksAgo(this IEnumerable<DataCollectorPerformance> dataCollectors,
            PerformanceStatusFilterDto filters) =>
            dataCollectors?.Where(dc => (dc.StatusThreeWeeksAgo == ReportingStatus.ReportingCorrectly && filters.ReportingCorrectly)
                || (dc.StatusThreeWeeksAgo == ReportingStatus.ReportingWithErrors && filters.ReportingWithErrors)
                || (dc.StatusThreeWeeksAgo == ReportingStatus.NotReporting && filters.NotReporting));

        public static IEnumerable<DataCollectorPerformance> FilterByStatusFourWeeksAgo(this IEnumerable<DataCollectorPerformance> dataCollectors,
            PerformanceStatusFilterDto filters) =>
            dataCollectors?.Where(dc => (dc.StatusFourWeeksAgo == ReportingStatus.ReportingCorrectly && filters.ReportingCorrectly)
                || (dc.StatusFourWeeksAgo == ReportingStatus.ReportingWithErrors && filters.ReportingWithErrors)
                || (dc.StatusFourWeeksAgo == ReportingStatus.NotReporting && filters.NotReporting));

        public static IEnumerable<DataCollectorPerformance> FilterByStatusFiveWeeksAgo(this IEnumerable<DataCollectorPerformance> dataCollectors,
            PerformanceStatusFilterDto filters) =>
            dataCollectors?.Where(dc => (dc.StatusFiveWeeksAgo == ReportingStatus.ReportingCorrectly && filters.ReportingCorrectly)
                || (dc.StatusFiveWeeksAgo == ReportingStatus.ReportingWithErrors && filters.ReportingWithErrors)
                || (dc.StatusFiveWeeksAgo == ReportingStatus.NotReporting && filters.NotReporting));

        public static IEnumerable<DataCollectorPerformance> FilterByStatusSixWeeksAgo(this IEnumerable<DataCollectorPerformance> dataCollectors,
            PerformanceStatusFilterDto filters) =>
            dataCollectors?.Where(dc => (dc.StatusSixWeeksAgo == ReportingStatus.ReportingCorrectly && filters.ReportingCorrectly)
                || (dc.StatusSixWeeksAgo == ReportingStatus.ReportingWithErrors && filters.ReportingWithErrors)
                || (dc.StatusSixWeeksAgo == ReportingStatus.NotReporting && filters.NotReporting));

        public static IEnumerable<DataCollectorPerformance> FilterByStatusSevenWeeksAgo(this IEnumerable<DataCollectorPerformance> dataCollectors,
            PerformanceStatusFilterDto filters) =>
            dataCollectors?.Where(dc => (dc.StatusSevenWeeksAgo == ReportingStatus.ReportingCorrectly && filters.ReportingCorrectly)
                || (dc.StatusSevenWeeksAgo == ReportingStatus.ReportingWithErrors && filters.ReportingWithErrors)
                || (dc.StatusSevenWeeksAgo == ReportingStatus.NotReporting && filters.NotReporting));

        public static IEnumerable<DataCollectorPerformance> FilterByStatusEightWeeksAgo(this IEnumerable<DataCollectorPerformance> dataCollectors,
            PerformanceStatusFilterDto filters) =>
            dataCollectors?.Where(dc => (dc.StatusEightWeeksAgo == ReportingStatus.ReportingCorrectly && filters.ReportingCorrectly)
                || (dc.StatusEightWeeksAgo == ReportingStatus.ReportingWithErrors && filters.ReportingWithErrors)
                || (dc.StatusEightWeeksAgo == ReportingStatus.NotReporting && filters.NotReporting));
    }
}
