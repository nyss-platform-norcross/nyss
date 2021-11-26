using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard.Dto
{
    public class NationalSocietyDashboardFiltersResponseDto
    {
        public IEnumerable<HealthRiskDto> HealthRisks { get; set; }

        public IEnumerable<OrganizationDto> Organizations { get; set; }

        public StructureResponseDto Locations { get; set; }

        public class OrganizationDto
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
