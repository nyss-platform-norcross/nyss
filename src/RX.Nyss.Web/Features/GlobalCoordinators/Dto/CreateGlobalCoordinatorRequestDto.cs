using FluentValidation;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.GlobalCoordinators.Dto
{
    public class CreateGlobalCoordinatorRequestDto
    {
        public string Email { get; set; }

        public string Name { get; set; }

        public string PhoneNumber { get; set; }

        public string Organization { get; set; }

        public string AdditionalPhoneNumber { get; set; }

        public override string ToString() => $"{nameof(Email)}: {Email}, {nameof(Name)}: {Name}, {nameof(PhoneNumber)}: {PhoneNumber}, {nameof(Organization)}: {Organization}";

        public class RegisterGlobalCoordinatorValidator : AbstractValidator<CreateGlobalCoordinatorRequestDto>
        {
            public RegisterGlobalCoordinatorValidator()
            {
                RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
                RuleFor(x => x.Email).NotEmpty().MaximumLength(100).EmailAddress();
                RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20).PhoneNumber();
                RuleFor(x => x.AdditionalPhoneNumber).MaximumLength(20);
                RuleFor(x => x.Organization).MaximumLength(100);
            }
        }
    }
}
