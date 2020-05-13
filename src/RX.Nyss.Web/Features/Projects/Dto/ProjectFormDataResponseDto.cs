using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Projects.Dto
{
    public class ProjectFormDataResponseDto
    {
        public IEnumerable<TimeZoneResponseDto> TimeZones { get; set; }
        public IEnumerable<ProjectHealthRiskResponseDto> HealthRisks { get; set; }
        public IEnumerable<Organization> Organizations { get; set; }

        public class Organization
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
