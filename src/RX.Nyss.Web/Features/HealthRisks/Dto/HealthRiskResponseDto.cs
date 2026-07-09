using System.Collections.Generic;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.SuspectedDiseases.Dto;

namespace RX.Nyss.Web.Features.HealthRisks.Dto
{
    public class HealthRiskResponseDto
    {
        public int Id { get; set; }

        public int HealthRiskCode { get; set; }

        public HealthRiskType HealthRiskType { get; set; }

        public int? AlertRuleCountThreshold { get; set; }

        public int? AlertRuleDaysThreshold { get; set; }

        public int? AlertRuleKilometersThreshold { get; set; }

        public IEnumerable<HealthRiskLanguageContentDto> LanguageContent { get; set; }

        public IEnumerable<HealthRiskSuspectedDisease> HealthRiskSuspectedDiseases { get; set; }

        public IEnumerable<SuspectedDiseaseListItemResponseDto> SuspectedDiseasesList { get; set; }

    }
}
