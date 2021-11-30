using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;

namespace RX.Nyss.Web.Features.ProjectDashboard.Dto
{
    public class ProjectDashboardFiltersResponseDto
    {
        public IEnumerable<HealthRiskDto> HealthRisks { get; set; }

        public IEnumerable<ProjectOrganizationDto> Organizations { get; set; }

        public StructureResponseDto Locations { get; set; }

        public class ProjectOrganizationDto
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
