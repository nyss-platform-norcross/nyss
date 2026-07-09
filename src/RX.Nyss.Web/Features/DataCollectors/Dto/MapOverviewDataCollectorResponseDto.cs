using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class MapOverviewDataCollectorResponseDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public ReportingStatus Status { get; set; }
        public bool IsAnonymized { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public bool Deployed { get; set; }
    }
}
