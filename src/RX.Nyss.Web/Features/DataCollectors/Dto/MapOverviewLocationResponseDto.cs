using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class MapOverviewLocationResponseDto
    {
        public int DataCollectorId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public LocationDto Location { get; set; } = new LocationDto();
        public int CountReportingCorrectly { get; set; }
        public int CountReportingWithErrors { get; set; }
        public int CountNotReporting { get; set; }
    }
}
