using FluentValidation;

namespace RX.Nyss.Web.Features.HealthRisks.Dto
{
    public class HealthRiskSuspectedDiseaseRequestDto
    {
        public int? Id { get; set; }

        public int SuspectedDiseaseId { get; set; }

        public class Validator : AbstractValidator<HealthRiskSuspectedDiseaseRequestDto>
        {
            public Validator()
            {
                RuleFor(s => s.SuspectedDiseaseId ).GreaterThan(0);
            }
        }
        
    }
}