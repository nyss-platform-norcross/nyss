using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard.Dto
{
    public class NationalSocietyDashboardFiltersResponseDto
    {
        public IEnumerable<HealthRiskDto> HealthRisks { get; set; } = new List<HealthRiskDto>();

        public IEnumerable<OrganizationDto> Organizations { get; set; } = new List<OrganizationDto>();

        public StructureResponseDto Locations { get; set; } = new StructureResponseDto();

        public class OrganizationDto
        {
            public int Id { get; set; }

            public string Name { get; set; } = string.Empty;
        }
    }
}
