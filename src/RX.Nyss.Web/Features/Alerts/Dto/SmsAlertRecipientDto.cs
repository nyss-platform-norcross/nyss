using FluentValidation;

namespace RX.Nyss.Web.Features.Alerts.Dto
{
    public class SmsAlertRecipientDto
    {
        public int? Id { get; set; }

        public string PhoneNumber { get; set; }

        public class Validator : AbstractValidator<SmsAlertRecipientDto>
        {
            public Validator()
            {
                RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id.HasValue);
                RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
            }
        }
    }
}
