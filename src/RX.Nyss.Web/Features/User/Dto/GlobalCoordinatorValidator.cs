using FluentValidation;

namespace RX.Nyss.Web.Features.User.Dto
{
    public class GlobalCoordinatorValidator : AbstractValidator<GlobalCoordinatorInDto>
    {
        public GlobalCoordinatorValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().MaximumLength(100).EmailAddress();
            RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        }
    }
}
