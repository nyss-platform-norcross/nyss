using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class DataCollectorPerformanceResponseDto
    {
        public List<Completeness> Completeness { get; set; }
        public PaginatedList<DataCollectorPerformance> Performance { get; set; }
    }

    public class DataCollectorPerformance
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string VillageName { get; set; }
        public int? DaysSinceLastReport { get; set; }
        public List<PerformanceInEpiWeek> PerformanceInEpiWeeks { get; set; }
    }

    public class DataCollectorCompleteness
    {
        public Completeness LastWeek { get; set; }
        public Completeness TwoWeeksAgo { get; set; }
        public Completeness ThreeWeeksAgo { get; set; }
        public Completeness FourWeeksAgo { get; set; }
        public Completeness FiveWeeksAgo { get; set; }
        public Completeness SixWeeksAgo { get; set; }
        public Completeness SevenWeeksAgo { get; set; }
        public Completeness EightWeeksAgo { get; set; }
    }

    public class PerformanceInEpiWeek
    {
        public int EpiWeek { get; set; }
        public int EpiYear { get; set; }
        public ReportingStatus? ReportingStatus { get; set; }
    }

    public class Completeness
    {
        public int EpiWeek { get; set; }
        public int EpiYear { get; set; }
        public int ActiveDataCollectors { get; set; }
        public int TotalDataCollectors { get; set; }
        public int Percentage { get; set; }
    }
}
