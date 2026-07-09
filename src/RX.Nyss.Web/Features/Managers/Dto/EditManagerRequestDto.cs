using FluentValidation;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.Managers.Dto
{
    public class EditManagerRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; } = string.Empty;
        public int? OrganizationId { get; set; }
        public int NationalSocietyId { get; set; }
        public int? ModemId { get; set; }

        public class EditManagerValidator : AbstractValidator<EditManagerRequestDto>
        {
            public EditManagerValidator()
            {
                RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
                RuleFor(m => m.Email).NotEmpty().MaximumLength(100);
                RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20).PhoneNumber();
                RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20).PhoneNumber().Unless(r => string.IsNullOrEmpty(r.AdditionalPhoneNumber));
                RuleFor(m => m.Organization).MaximumLength(100);
                RuleFor(m => m.ModemId)
                    .GreaterThan(0)
                    .When(m => m.ModemId.HasValue);
            }
        }
    }
}
