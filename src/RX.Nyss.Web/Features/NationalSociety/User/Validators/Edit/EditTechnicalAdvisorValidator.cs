using FluentValidation;
using RX.Nyss.Web.Features.NationalSociety.User.Dto.Edit;

namespace RX.Nyss.Web.Features.NationalSociety.User.Validators.Edit
{
    public class EditTechnicalAdvisorValidator : AbstractValidator<EditTechnicalAdvisorRequestDto>
    {
        public EditTechnicalAdvisorValidator()
        {
            RuleFor(m => m.Organization).NotEmpty().MaximumLength(100);
        }
    }
}
