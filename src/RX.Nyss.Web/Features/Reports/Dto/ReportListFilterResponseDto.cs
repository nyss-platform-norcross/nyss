using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;

namespace RX.Nyss.Web.Features.Reports.Dto
{
    public class ReportListFilterResponseDto
    {
        public IEnumerable<HealthRiskDto> HealthRisks { get; set; }
        public StructureResponseDto Locations { get; set; }
    }
}
