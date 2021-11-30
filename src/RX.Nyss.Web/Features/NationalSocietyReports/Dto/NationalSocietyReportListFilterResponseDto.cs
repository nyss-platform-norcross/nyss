using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;

namespace RX.Nyss.Web.Features.NationalSocietyReports.Dto
{
    public class NationalSocietyReportListFilterResponseDto
    {
        public IEnumerable<HealthRiskDto> HealthRisks { get; set; }
        public StructureResponseDto Locations { get; set; }
    }
}
