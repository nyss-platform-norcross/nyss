using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.DataCollector.Dto
{
    public class DataCollectorPerformanceResponseDto
    {
        public string Name { get; set; }
        public int DaysSinceLastReport { get; set; }
        public DataCollectorStatus StatusLastWeek { get; set; }
        public DataCollectorStatus StatusTwoWeeksAgo { get; set; }
        public DataCollectorStatus StatusThreeWeeksAgo { get; set; }
        public DataCollectorStatus StatusFourWeeksAgo { get; set; }
        public DataCollectorStatus StatusFiveWeeksAgo { get; set; }
        public DataCollectorStatus StatusSixWeeksAgo { get; set; }
        public DataCollectorStatus StatusSevenWeeksAgo { get; set; }
        public DataCollectorStatus StatusEightWeeksAgo { get; set; }
    }
}
