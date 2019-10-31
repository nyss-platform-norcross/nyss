using FluentValidation;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.NationalSociety.User.DataManager.Dto
{
    public class EditDataManagerRequestDto:IEditNationalSocietyUserRequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; }
    }

    public class EditDataManagerValidator : AbstractValidator<EditDataManagerRequestDto>
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
