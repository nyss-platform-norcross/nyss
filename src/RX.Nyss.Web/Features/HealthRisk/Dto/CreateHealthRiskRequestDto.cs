using System.Collections.Generic;
using FluentValidation;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.HealthRisk.Dto
{
    public class CreateHealthRiskRequestDto
    {
        public int HealthRiskCode { get; set; }

        public HealthRiskType HealthRiskType { get; set; }

        public int AlertRuleCountThreshold { get; set; }

        public int AlertRuleHoursThreshold { get; set; }

        public int AlertRuleMetersThreshold { get; set; }
        public List<HealthRiskLanguageContentDto> LanguageContent { get; set; }

        public class Validator : AbstractValidator<CreateHealthRiskRequestDto>
        {
            public Validator()
            {
                RuleFor(hr => hr.HealthRiskCode).GreaterThan(0);
                RuleFor(hr => hr.HealthRiskType).IsInEnum();
                RuleFor(hr => hr.LanguageContent).NotEmpty();
                RuleFor(hr => hr.AlertRuleCountThreshold).GreaterThanOrEqualTo(0);
                RuleFor(hr => hr.AlertRuleHoursThreshold).GreaterThanOrEqualTo(0);
                RuleFor(hr => hr.AlertRuleMetersThreshold).GreaterThanOrEqualTo(0);
                RuleForEach(hr => hr.LanguageContent).SetValidator(new HealthRiskLanguageContentDto.Validator());
            }
        }
    }
}
