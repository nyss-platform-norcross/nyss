using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.NationalSocietyReports.Dto
{
    public class NationalSocietyReportListFilterResponseDto
    {
        public IEnumerable<HealthRiskDto> HealthRisks { get; set; }
    }
}
