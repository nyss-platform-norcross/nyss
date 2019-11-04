using FluentValidation;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.DataManager.Dto
{
    public class EditDataManagerRequestDto
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; }
    }

    public class EditDataManagerValidator : AbstractValidator<EditDataManagerRequestDto>
    {
        public EditDataManagerValidator()
        {
            RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
            RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20).PhoneNumber();
            RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20).PhoneNumber();
        }
    }
}
