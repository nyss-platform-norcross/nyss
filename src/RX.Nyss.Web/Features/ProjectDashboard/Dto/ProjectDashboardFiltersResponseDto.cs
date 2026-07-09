using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;

namespace RX.Nyss.Web.Features.ProjectDashboard.Dto
{
    public class ProjectDashboardFiltersResponseDto
    {
        public IEnumerable<HealthRiskDto> HealthRisks { get; set; } = new List<HealthRiskDto>();

        public IEnumerable<ProjectOrganizationDto> Organizations { get; set; } = new List<ProjectOrganizationDto>();

        public StructureResponseDto Locations { get; set; } = new StructureResponseDto();

        public class ProjectOrganizationDto
        {
            public int Id { get; set; }

            public string Name { get; set; } = string.Empty;
        }
    }
}
