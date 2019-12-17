using FluentValidation;

namespace RX.Nyss.Web.Features.Alerts.Dto
{
    public class EmailAlertRecipientDto
    {
        public int? Id { get; set; }

        public string Email { get; set; }

        public class Validator : AbstractValidator<EmailAlertRecipientDto>
        {
            public Validator()
            {
                RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id.HasValue);
                RuleFor(x => x.Email).NotEmpty().MaximumLength(100).EmailAddress();
            }
        }
    }
}
