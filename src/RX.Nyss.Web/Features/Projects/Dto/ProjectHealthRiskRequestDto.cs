using FluentValidation;

namespace RX.Nyss.Web.Features.Projects.Dto
{
    public class ProjectHealthRiskRequestDto
    {
        public int? Id { get; set; }

        public int HealthRiskId { get; set; }

        public int? AlertRuleCountThreshold { get; set; }

        public int? AlertRuleDaysThreshold { get; set; }

        public int? AlertRuleKilometersThreshold { get; set; }

        public string FeedbackMessage { get; set; }

        public string CaseDefinition { get; set; }

        public class Validator : AbstractValidator<ProjectHealthRiskRequestDto>
        {
            public Validator()
            {
                RuleFor(phr => phr.Id).GreaterThan(0).When(phr => phr.Id.HasValue);
                RuleFor(phr => phr.HealthRiskId).GreaterThan(0);
                RuleFor(phr => phr.AlertRuleCountThreshold).GreaterThanOrEqualTo(0).When(phr => phr.AlertRuleCountThreshold.HasValue);
                RuleFor(phr => phr.AlertRuleDaysThreshold)
                    .NotEmpty().When(hr => hr.AlertRuleCountThreshold > 1)
                    .InclusiveBetween(1, 365).When(hr => hr.AlertRuleDaysThreshold.HasValue);
                RuleFor(phr => phr.AlertRuleKilometersThreshold)
                    .NotEmpty().When(hr => hr.AlertRuleCountThreshold > 1)
                    .InclusiveBetween(1, 9999).When(hr => hr.AlertRuleDaysThreshold.HasValue);
                RuleFor(phr => phr.FeedbackMessage).MaximumLength(160);
                RuleFor(phr => phr.CaseDefinition).MaximumLength(500);
            }
        }
    }
}
