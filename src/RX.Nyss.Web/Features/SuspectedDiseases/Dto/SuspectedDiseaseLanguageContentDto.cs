using FluentValidation;

namespace RX.Nyss.Web.Features.SuspectedDiseases.Dto
{
    public class SuspectedDiseaseLanguageContentDto
    {
        public int LanguageId { get; set; }

        public string Name { get; set; }

        public class Validator : AbstractValidator<SuspectedDiseaseLanguageContentDto>
        {
            public Validator()
            {
                RuleFor(h => h.Name).NotEmpty().MaximumLength(100);
                RuleFor(h => h.LanguageId).GreaterThan(0);
            }
        }
    }
}
