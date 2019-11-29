using System.Collections.Generic;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;

namespace RX.Nyss.Web.Features.ProjectDashboard.Dto
{
    public class ProjectDashboardFiltersResponseDto
    {
        public IEnumerable<StructureResponseDto.StructureRegionDto> Regions { get; set; }

        public IEnumerable<HealthRiskDto> HealthRisks { get; set; }

        public class HealthRiskDto
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
