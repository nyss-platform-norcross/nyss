using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.DataCollector.Dto
{
    public class MapOverviewLocationResponseDto
    {
        public LocationDto Location { get; set; }
        public int CountReportingCorrectly { get; set; }
        public int CountReportingWithErrors { get; set; }
        public int CountNotReporting { get; set; }
    }
}
