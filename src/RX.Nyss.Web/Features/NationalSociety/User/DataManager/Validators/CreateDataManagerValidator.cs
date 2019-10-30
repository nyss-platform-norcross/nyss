using FluentValidation;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.NationalSociety.User.DataManager.Dto;

namespace RX.Nyss.Web.Features.NationalSociety.User.DataManager.Validators
{
    public class CreateDataManagerValidator : AbstractValidator<CreateDataManagerRequestDto>
    {
        public CreateDataManagerValidator()
        {
            RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
            RuleFor(m => m.Email).NotEmpty().MaximumLength(100).EmailAddress();
            RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20);
            RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20);
        }
    }
}
