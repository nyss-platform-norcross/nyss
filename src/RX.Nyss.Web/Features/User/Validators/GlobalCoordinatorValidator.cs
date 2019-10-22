using FluentValidation;
using RX.Nyss.Web.Features.User.Requests;

namespace RX.Nyss.Web.Features.User.Validators
{
    public class GlobalCoordinatorValidator : AbstractValidator<RegisterGlobalCoordinatorRequest>
    {
        public GlobalCoordinatorValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().MaximumLength(100).EmailAddress();
            RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        }
    }
}
