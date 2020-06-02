using FluentValidation;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.Coordinators.Dto
{
    public class EditCoordinatorRequestDto
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; }
        public int OrganizationId { get; set; }

        public class EditCoordinatorValidator : AbstractValidator<EditCoordinatorRequestDto>
        {
            public EditCoordinatorValidator()
            {
                RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
                RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20).PhoneNumber();
                RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20).PhoneNumber()
                    .Unless(r => string.IsNullOrEmpty(r.AdditionalPhoneNumber));
                RuleFor(m => m.Organization).MaximumLength(100);
                RuleFor(m => m.OrganizationId).Must(m => m > 0);
            }
        }
    }
}
