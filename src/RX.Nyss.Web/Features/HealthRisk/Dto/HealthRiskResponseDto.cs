using System.Collections.Generic;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Alert.Dto;

namespace RX.Nyss.Web.Features.HealthRisk.Dto
{
    public class HealthRiskResponseDto
    {
        public int Id { get; set; }

        public int HealthRiskCode { get; set; }

        public HealthRiskType HealthRiskType { get; set; }

        public int? AlertRuleCountThreshold { get; set; }

        public int? AlertRuleDaysThreshold { get; set; }

        public int? AlertRuleMetersThreshold { get; set; }

        public IEnumerable<AlertRecipientDto> AlertRecipients { get; set; }

        public IEnumerable<HealthRiskLanguageContentDto> LanguageContent { get; set; }
    }
}
