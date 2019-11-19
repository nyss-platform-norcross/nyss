using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Project.Dto
{
    public class ProjectFormDataResponseDto
    {
        public IEnumerable<TimeZoneResponseDto> TimeZones { get; set; }
        public IEnumerable<ProjectHealthRiskResponseDto> HealthRisks { get; set; }
    }
}
