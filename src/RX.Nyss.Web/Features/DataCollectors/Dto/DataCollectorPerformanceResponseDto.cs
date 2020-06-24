using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class DataCollectorPerformanceResponseDto
    {
        public string Name { get; set; }
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
}
