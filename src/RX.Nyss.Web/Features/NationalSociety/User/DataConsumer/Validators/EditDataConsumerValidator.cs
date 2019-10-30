using FluentValidation;
using RX.Nyss.Web.Features.NationalSociety.User.DataConsumer.Dto;

namespace RX.Nyss.Web.Features.NationalSociety.User.DataConsumer.Validators
{
    public class EditDataConsumerValidator : AbstractValidator<EditDataConsumerRequestDto>
    {
        public EditDataConsumerValidator()
        {
            RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
            RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20);
            RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20);
            RuleFor(m => m.Organization).NotEmpty().MaximumLength(100);
        }
    }
}
