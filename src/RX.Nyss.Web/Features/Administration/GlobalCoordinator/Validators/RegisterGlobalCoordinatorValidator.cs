using FluentValidation;
using RX.Nyss.Web.Features.Administration.GlobalCoordinator.Dto;

namespace RX.Nyss.Web.Features.User.Validators
{
    public class RegisterGlobalCoordinatorValidator : AbstractValidator<RegisterGlobalCoordinatorRequestDto>
    {
        public RegisterGlobalCoordinatorValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().MaximumLength(100).EmailAddress();
            RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        }
    }
}
