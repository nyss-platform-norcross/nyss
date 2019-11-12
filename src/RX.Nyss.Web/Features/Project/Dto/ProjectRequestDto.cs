using System.Collections.Generic;
using FluentValidation;
using RX.Nyss.Web.Features.Alert.Dto;

namespace RX.Nyss.Web.Features.Project.Dto
{
    public class ProjectRequestDto
    {
        public string Name { get; set; }

        public string TimeZone { get; set; }

        public IEnumerable<ProjectHealthRiskRequestDto> HealthRisks { get; set; }

        public IEnumerable<AlertRecipientDto> AlertRecipients { get; set; }

        public class Validator : AbstractValidator<ProjectRequestDto>
        {
            public Validator()
            {
                RuleFor(p => p.Name).NotEmpty().MaximumLength(200);
                RuleFor(p => p.Name).MaximumLength(50);
                RuleFor(p => p.HealthRisks).NotNull();
                RuleFor(p => p.AlertRecipients).NotNull();
                RuleForEach(p => p.HealthRisks).SetValidator(new ProjectHealthRiskRequestDto.Validator());
                RuleForEach(p => p.AlertRecipients).SetValidator(new AlertRecipientDto.Validator());
            }
        }
    }
}
