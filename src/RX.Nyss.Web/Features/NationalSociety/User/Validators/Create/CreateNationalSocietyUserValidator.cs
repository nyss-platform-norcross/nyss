using FluentValidation;
using RX.Nyss.Web.Features.NationalSociety.User.Dto.Create;

namespace RX.Nyss.Web.Features.NationalSociety.User.Validators.Create
{
    public class CreateNationalSocietyUserValidator : AbstractValidator<CreateNationalSocietyUserRequestDto>
    {
        public CreateNationalSocietyUserValidator()
        {
            RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
            RuleFor(m => m.Email).NotEmpty().MaximumLength(100).EmailAddress();
            RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20);
            RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20);
        }
    }
}
