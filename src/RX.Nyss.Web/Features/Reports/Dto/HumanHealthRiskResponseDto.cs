using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.Reports.Dto
{
    public class HumanHealthRiskResponseDto
    {
        public IEnumerable<HealthRiskDto> HealthRisks { get; set; }
    }
}
