using System.Collections.Generic;
using FluentValidation;
using RX.Nyss.Web.Features.Alerts.Dto;

namespace RX.Nyss.Web.Features.Projects.Dto
{
    public class EditProjectRequestDto
    {
        public string Name { get; set; }

        public bool AllowMultipleOrganizations { get; set; }

        public string TimeZoneId { get; set; }

        public IEnumerable<ProjectHealthRiskRequestDto> HealthRisks { get; set; }

        public class Validator : AbstractValidator<EditProjectRequestDto>
        {
            public Validator()
            {
                RuleFor(p => p.Name).NotEmpty().MaximumLength(200);
                RuleFor(p => p.Name).MaximumLength(50);
                RuleFor(p => p.HealthRisks).NotNull();
                RuleForEach(p => p.HealthRisks).SetValidator(new ProjectHealthRiskRequestDto.Validator());
            }
        }
    }
}
