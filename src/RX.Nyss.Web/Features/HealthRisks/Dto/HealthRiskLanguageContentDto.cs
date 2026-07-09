using FluentValidation;

namespace RX.Nyss.Web.Features.HealthRisks.Dto
{
    public class HealthRiskLanguageContentDto
    {
        public int LanguageId { get; set; }

        public string Name { get; set; }

        public string FeedbackMessage { get; set; }

        public string CaseDefinition { get; set; }

        public class Validator : AbstractValidator<HealthRiskLanguageContentDto>
        {
            public Validator()
            {
                RuleFor(h => h.Name).NotEmpty().MaximumLength(100);
                RuleFor(h => h.CaseDefinition).NotEmpty().MaximumLength(500);
                RuleFor(h => h.FeedbackMessage).NotEmpty().MaximumLength(160);
                RuleFor(h => h.LanguageId).GreaterThan(0);
            }
        }
    }
}
