using FluentValidation;

namespace RX.Nyss.Web.Features.Alerts.Dto
{
    public class AlertNotificationRecipientDto
    {
        public int? Id { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        public string Organization { get; set; }


        public class Validator : AbstractValidator<AlertNotificationRecipientDto>
        {
            public Validator()
            {
                RuleFor(anr => anr.Id).GreaterThan(0).When(anr => anr.Id.HasValue);
                RuleFor(anr => anr.PhoneNumber).MaximumLength(20).NotEmpty().When(anr => string.IsNullOrEmpty(anr.Email));
                RuleFor(anr => anr.Email).MaximumLength(100).NotEmpty().When(anr => string.IsNullOrEmpty(anr.PhoneNumber));
                RuleFor(anr => anr.Role).NotEmpty().MaximumLength(100);
                RuleFor(anr => anr.Organization).NotEmpty().MaximumLength(100);
            }
        }
    }
}
