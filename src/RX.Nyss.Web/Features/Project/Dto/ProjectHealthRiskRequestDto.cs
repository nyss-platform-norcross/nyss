using FluentValidation;

namespace RX.Nyss.Web.Features.Project.Dto
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
                RuleFor(phr => phr.Id).GreaterThanOrEqualTo(0).When(phr => phr.Id.HasValue);
                RuleFor(phr => phr.AlertRuleCountThreshold).GreaterThanOrEqualTo(0).When(phr => phr.AlertRuleCountThreshold.HasValue);
                RuleFor(phr => phr.AlertRuleDaysThreshold).GreaterThanOrEqualTo(0).When(phr => phr.AlertRuleDaysThreshold.HasValue);
                RuleFor(phr => phr.AlertRuleKilometersThreshold).GreaterThanOrEqualTo(0).When(phr => phr.AlertRuleKilometersThreshold.HasValue);
                RuleFor(phr => phr.FeedbackMessage).MaximumLength(160);
                RuleFor(phr => phr.CaseDefinition).MaximumLength(500);
            }
        }
    }
}
