using FluentValidation;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.NationalSociety.User.DataManager.Validators
{
    public class EditDataManagerValidator : AbstractValidator<DataManagerUser>
    {
        public EditDataManagerValidator()
        {
            RuleFor(m => m.Id).NotEmpty();
            RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
            RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20);
            RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20);
        }
    }
}
