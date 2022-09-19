using FluentValidation;

namespace RX.Nyss.Web.Features.HealthRisks.Dto
{
    public class HealthRiskSuspectedDiseaseDto
    {
        public int SuspectedDiseaseId { get; set; }

        public string SuspectedDiseaseName { get; set; }

        public class Validator : AbstractValidator<HealthRiskSuspectedDiseaseDto>
        {
            public Validator()
            {
                RuleFor(s => s.SuspectedDiseaseId ).GreaterThan(0);
                RuleFor(s => s.SuspectedDiseaseName).NotEmpty().MaximumLength(500);
            }
        }
        
    }
}