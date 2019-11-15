using FluentValidation;

namespace RX.Nyss.Web.Features.Alert.Dto
{
    public class AlertRecipientDto
    {
        public int? Id { get; set; }

        public string Email { get; set; }

        public class Validator : AbstractValidator<AlertRecipientDto>
        {
            public Validator()
            {
                RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id.HasValue);
                RuleFor(x => x.Email).NotEmpty().MaximumLength(100).EmailAddress();
            }
        }
    }
}
