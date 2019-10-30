using FluentValidation;
using RX.Nyss.Web.Features.NationalSociety.User.TechnicalAdvisor.Dto;

namespace RX.Nyss.Web.Features.NationalSociety.User.TechnicalAdvisor.Validators
{
    public class EditTechnicalAdvisorValidator : AbstractValidator<EditTechnicalAdvisorRequestDto>
    {
        public EditTechnicalAdvisorValidator()
        {
            RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
            RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20);
            RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20);
            RuleFor(m => m.Organization).NotEmpty().MaximumLength(100);
        }
    }
}
