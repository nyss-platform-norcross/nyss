using FluentValidation;

namespace RX.Nyss.Web.Features.Alert.Dto
{
    public class AlertRecipientDto
    {
        public int? Id { get; set; }

        public string EmailAddress { get; set; }

        public class Validator : AbstractValidator<AlertRecipientDto>
        {
            public Validator()
            {
                RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id.HasValue);
                RuleFor(x => x.EmailAddress).NotEmpty().MaximumLength(100).EmailAddress();
            }
        }
    }
}
