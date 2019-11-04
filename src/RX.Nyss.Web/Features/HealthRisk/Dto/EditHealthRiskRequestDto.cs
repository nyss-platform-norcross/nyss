using System.Collections.Generic;
using FluentValidation;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.HealthRisk.Dto
{
    public class EditHealthRiskRequestDto
    {
        public int Id { get; set; }

        public int HealthRiskCode { get; set; }

        public HealthRiskType HealthRiskType { get; set; }

        public int? AlertRuleCountThreshold { get; set; }

        public int? AlertRuleDaysThreshold { get; set; }

        public int? AlertRuleMetersThreshold { get; set; }

        public IEnumerable<HealthRiskLanguageContentDto> LanguageContent { get; set; }

        public class Validator : AbstractValidator<EditHealthRiskRequestDto>
        {
            public Validator()
            {
                RuleFor(hr => hr.Id).GreaterThan(0);
                RuleFor(hr => hr.HealthRiskCode).GreaterThan(0);
                RuleFor(hr => hr.HealthRiskType).IsInEnum();
                RuleFor(hr => hr.LanguageContent).NotEmpty();
                RuleForEach(hr => hr.LanguageContent).SetValidator(new HealthRiskLanguageContentDto.Validator());
            }
        }
    }
}
