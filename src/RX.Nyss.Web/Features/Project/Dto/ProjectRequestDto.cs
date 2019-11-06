using System.Collections.Generic;
using FluentValidation;
using RX.Nyss.Web.Features.HealthRisk.Dto;

namespace RX.Nyss.Web.Features.Project.Dto
{
    public class ProjectRequestDto
    {
        public string Name { get; set; }

        public string TimeZone { get; set; }

        public IEnumerable<HealthRiskRequestDto> HealthRisks;

        public class Validator : AbstractValidator<ProjectRequestDto>
        {
            public Validator()
            {
                RuleFor(p => p.Name).NotEmpty();
                RuleForEach(p => p.HealthRisks).SetValidator(new HealthRiskRequestDto.Validator());
            }
        }
    }
}
