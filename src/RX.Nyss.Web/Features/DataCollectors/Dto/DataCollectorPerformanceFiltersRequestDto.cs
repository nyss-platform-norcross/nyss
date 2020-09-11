using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class DataCollectorPerformanceFiltersRequestDto
    {
        public AreaDto Area { get; set; }

        public string Name { get; set; }

        public PerformanceStatusFilterDto LastWeek { get; set; }

        public PerformanceStatusFilterDto TwoWeeksAgo { get; set; }

        public PerformanceStatusFilterDto ThreeWeeksAgo { get; set; }

        public PerformanceStatusFilterDto FourWeeksAgo { get; set; }

        public PerformanceStatusFilterDto FiveWeeksAgo { get; set; }

        public PerformanceStatusFilterDto SixWeeksAgo { get; set; }

        public PerformanceStatusFilterDto SevenWeeksAgo { get; set; }

        public PerformanceStatusFilterDto EightWeeksAgo { get; set; }
    }

    public class PerformanceStatusFilterDto
    {
        public bool ReportingCorrectly { get; set; }

        public bool ReportingWithErrors { get; set; }

        public bool NotReporting { get; set; }
    }
}
