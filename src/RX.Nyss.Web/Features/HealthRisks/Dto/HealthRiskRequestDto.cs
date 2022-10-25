using System.Collections.Generic;
using FluentValidation;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.HealthRisks.Dto
{
    public class HealthRiskRequestDto
    {
        public int HealthRiskCode { get; set; }

        public HealthRiskType HealthRiskType { get; set; }

        public int? AlertRuleCountThreshold { get; set; }

        public int? AlertRuleDaysThreshold { get; set; }

        public int? AlertRuleKilometersThreshold { get; set; }

        public IEnumerable<HealthRiskSuspectedDiseaseRequestDto> HealthRiskSuspectedDiseasesRequest { get; set; }

        public IEnumerable<HealthRiskLanguageContentDto> LanguageContent { get; set; }

        public class Validator : AbstractValidator<HealthRiskRequestDto>
        {
            public Validator()
            {
                RuleFor(hr => hr.HealthRiskCode).GreaterThan(0);
                RuleFor(hr => hr.HealthRiskType).IsInEnum();
                RuleFor(hr => hr.AlertRuleCountThreshold).GreaterThanOrEqualTo(0).When(hr => hr.AlertRuleCountThreshold.HasValue);
                RuleFor(hr => hr.AlertRuleDaysThreshold)
                    .NotEmpty().When(hr => hr.AlertRuleCountThreshold > 1)
                    .InclusiveBetween(1, 365).When(hr => hr.AlertRuleDaysThreshold.HasValue);
                RuleFor(hr => hr.AlertRuleKilometersThreshold)
                    .NotEmpty().When(hr => hr.AlertRuleCountThreshold > 1)
                    .InclusiveBetween(1, 9999).When(hr => hr.AlertRuleKilometersThreshold.HasValue);
                RuleFor(hr => hr.LanguageContent).NotEmpty();
            }
        }
    }
}
