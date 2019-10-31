using FluentValidation;

namespace RX.Nyss.Web.Features.HealthRisk
{
    public class HealthRiskDescriptions
    {
        public int LanguageId { get; set; }
        public string FeedbackMessage { get; set; }
        public string CaseDefinition { get; set; }

        public class Validator : AbstractValidator<HealthRiskDescriptions>
        {
            public Validator()
            {
                RuleFor(h => h.CaseDefinition).NotEmpty().MaximumLength(500);
                RuleFor(h => h.FeedbackMessage).NotEmpty().MaximumLength(160);
                RuleFor(h => h.LanguageId).GreaterThan(0);
            }
        }
    }
}
