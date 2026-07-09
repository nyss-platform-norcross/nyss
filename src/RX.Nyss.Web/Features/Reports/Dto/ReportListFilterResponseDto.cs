using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;

namespace RX.Nyss.Web.Features.Reports.Dto
{
    public class ReportListFilterResponseDto
    {
        public IEnumerable<HealthRiskDto> HealthRisks { get; set; } = new List<HealthRiskDto>();
        public StructureResponseDto Locations { get; set; } = new StructureResponseDto();
    }
}
