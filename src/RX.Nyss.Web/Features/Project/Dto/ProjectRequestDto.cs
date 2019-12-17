using System.Collections.Generic;
using FluentValidation;
using RX.Nyss.Web.Features.Alerts.Dto;

namespace RX.Nyss.Web.Features.Project.Dto
{
    public class ProjectRequestDto
    {
        public string Name { get; set; }

        public string TimeZoneId { get; set; }

        public IEnumerable<ProjectHealthRiskRequestDto> HealthRisks { get; set; }

        public IEnumerable<EmailAlertRecipientDto> EmailAlertRecipients { get; set; }

        public IEnumerable<SmsAlertRecipientDto> SmsAlertRecipients { get; set; }

        public class Validator : AbstractValidator<ProjectRequestDto>
        {
            public Validator()
            {
                RuleFor(p => p.Name).NotEmpty().MaximumLength(200);
                RuleFor(p => p.Name).MaximumLength(50);
                RuleFor(p => p.HealthRisks).NotNull();
                RuleFor(p => p.EmailAlertRecipients).NotNull();
                RuleFor(p => p.SmsAlertRecipients).NotNull();
                RuleForEach(p => p.HealthRisks).SetValidator(new ProjectHealthRiskRequestDto.Validator());
                RuleForEach(p => p.EmailAlertRecipients).SetValidator(new EmailAlertRecipientDto.Validator());
                RuleForEach(p => p.SmsAlertRecipients).SetValidator(new SmsAlertRecipientDto.Validator());
            }
        }
    }
}
