using FluentValidation;

namespace RX.Nyss.Web.Features.HealthRisks.Dto
{
    public class HealthRiskSuspectedDiseaseRequestDto
    {
        public int SuspectedDiseaseId { get; set; }

        public int SuspectedDiseaseCode { get; set; }

        public class Validator : AbstractValidator<HealthRiskSuspectedDiseaseRequestDto>
        {
            public Validator()
            {
                RuleFor(s => s.SuspectedDiseaseId ).GreaterThan(0);
            }
        }
    }
}