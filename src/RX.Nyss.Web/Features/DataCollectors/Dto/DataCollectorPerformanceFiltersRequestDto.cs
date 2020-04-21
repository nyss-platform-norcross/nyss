using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class DataCollectorPerformanceFiltersRequestDto
    {
        public AreaDto Area { get; set; }

        public bool ReportingCorrectly { get; set; }

        public bool ReportingWithErrors { get; set; }

        public bool NotReporting { get; set; }
    }
}
