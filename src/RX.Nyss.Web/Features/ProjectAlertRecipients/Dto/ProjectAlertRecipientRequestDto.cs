using System.Collections.Generic;
using FluentValidation;

namespace RX.Nyss.Web.Features.ProjectAlertRecipients.Dto
{
    public class ProjectAlertRecipientRequestDto
    {
        public int? Id { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        public string Organization { get; set; }

        public int? OrganizationId { get; set; }
        public int? ModemId { get; set; }

        public List<SupervisorAlertRecipientRequestDto> Supervisors { get; set; }

        public List<int> HealthRisks { get; set; }

        public class Validator : AbstractValidator<ProjectAlertRecipientRequestDto>
        {
            public Validator()
            {
                RuleFor(anr => anr.Id).GreaterThan(0).When(anr => anr.Id.HasValue);
                RuleFor(anr => anr.PhoneNumber).MaximumLength(20).NotEmpty().When(anr => string.IsNullOrEmpty(anr.Email));
                RuleFor(anr => anr.Email).MaximumLength(100).NotEmpty().When(anr => string.IsNullOrEmpty(anr.PhoneNumber));
                RuleFor(anr => anr.Role).NotEmpty().MaximumLength(100);
                RuleFor(anr => anr.Organization).NotEmpty().MaximumLength(100);
                RuleFor(anr => anr.ModemId).GreaterThan(0).When(anr => anr.ModemId.HasValue);
            }
        }
    }
}
