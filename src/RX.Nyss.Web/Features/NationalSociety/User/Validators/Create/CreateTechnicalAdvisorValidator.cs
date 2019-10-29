using FluentValidation;
using RX.Nyss.Web.Features.NationalSociety.User.Dto.Create;

namespace RX.Nyss.Web.Features.NationalSociety.User.Validators.Create
{
    public class CreateTechnicalAdvisorValidator : AbstractValidator<CreateTechnicalAdvisorRequestDto>
    {
        public CreateTechnicalAdvisorValidator()
        {
            RuleFor(m => m.Organization).NotEmpty().MaximumLength(100);
        }
    }
}
