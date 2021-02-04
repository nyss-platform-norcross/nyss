using FluentValidation;

namespace RX.Nyss.Web.Features.Users.Dto
{
    public class AddExistingUserToNationalSocietyRequestDto
    {
        public string Email { get; set; }
        public int? ModemId { get; set; }
    }

    public class EditGlobalCoordinatorValidator : AbstractValidator<AddExistingUserToNationalSocietyRequestDto>
    {
        public EditGlobalCoordinatorValidator()
        {
            RuleFor(m => m.Email).NotEmpty().MaximumLength(100).EmailAddress();
            RuleFor(m => m.ModemId).GreaterThan(0).When(m => m.ModemId.HasValue);
        }
    }
}
