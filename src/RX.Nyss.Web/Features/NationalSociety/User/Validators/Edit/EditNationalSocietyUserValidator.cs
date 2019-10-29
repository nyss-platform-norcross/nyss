using FluentValidation;
using RX.Nyss.Web.Features.NationalSociety.User.Dto.Edit;

namespace RX.Nyss.Web.Features.NationalSociety.User.Validators.Edit
{
    public class EditNationalSocietyUserValidator : AbstractValidator<EditNationalSocietyUserRequestDto>
    {
        public EditNationalSocietyUserValidator()
        {
            RuleFor(m => m.Id).NotEmpty();
            RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
            RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20);
            RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20);
        }
    }
}
