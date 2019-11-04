using FluentValidation;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.DataManager.Dto
{
    public class CreateDataManagerRequestDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; }

        public class CreateDataManagerValidator : AbstractValidator<CreateDataManagerRequestDto>
        {
            public CreateDataManagerValidator()
            {
                RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
                RuleFor(m => m.Email).NotEmpty().MaximumLength(100).EmailAddress();
                RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20).PhoneNumber();
                RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20).PhoneNumber();
            }
        }
    }
}
