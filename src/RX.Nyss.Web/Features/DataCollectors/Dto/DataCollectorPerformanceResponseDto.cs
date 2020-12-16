using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class DataCollectorPerformanceResponseDto
    {
        public DataCollectorCompleteness Completeness { get; set; }
        public PaginatedList<DataCollectorPerformance> Performance { get; set; }
    }

    public class DataCollectorPerformance
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string VillageName { get; set; }
        public int? DaysSinceLastReport { get; set; }
        public ReportingStatus StatusLastWeek { get; set; }
        public ReportingStatus StatusTwoWeeksAgo { get; set; }
        public ReportingStatus StatusThreeWeeksAgo { get; set; }
        public ReportingStatus StatusFourWeeksAgo { get; set; }
        public ReportingStatus StatusFiveWeeksAgo { get; set; }
        public ReportingStatus StatusSixWeeksAgo { get; set; }
        public ReportingStatus StatusSevenWeeksAgo { get; set; }
        public ReportingStatus StatusEightWeeksAgo { get; set; }
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

    public class Completeness
    {
        public int ActiveDataCollectors { get; set; }
        public int TotalDataCollectors { get; set; }
        public int Percentage { get; set; }
    }
}
