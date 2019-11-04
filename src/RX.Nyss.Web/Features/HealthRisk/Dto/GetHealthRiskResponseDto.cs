using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.HealthRisk.Dto
{
    public class GetHealthRiskResponseDto
    {
        public int Id { get; set; }
        public int HealthRiskCode { get; set; }

        public HealthRiskType HealthRiskType { get; set; }
        public int AlertRuleId { get; set; }

        public int AlertRuleCountThreshold { get; set; }

        public int AlertRuleHoursThreshold { get; set; }

        public int AlertRuleMetersThreshold { get; set; }
        public IEnumerable<HealthRiskLanguageContentDto> LanguageContent { get; set; }
    }
}
