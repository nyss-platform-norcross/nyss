using FluentValidation;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.TechnicalAdvisors.Dto
{
    public class EditTechnicalAdvisorRequestDto
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; }
        public int? OrganizationId { get; set; }
        public int NationalSocietyId { get; set; }

        public class EditTechnicalAdvisorValidator : AbstractValidator<EditTechnicalAdvisorRequestDto>
        {
            public EditTechnicalAdvisorValidator()
            {
                RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
                RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20).PhoneNumber();
                RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20).PhoneNumber().Unless(r => string.IsNullOrEmpty(r.AdditionalPhoneNumber));
                RuleFor(m => m.Organization).MaximumLength(100);
            }
        }
    }
}
