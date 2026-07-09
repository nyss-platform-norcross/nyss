using FluentValidation;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Features.Organizations;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Coordinators.Dto
{
    public class CreateCoordinatorRequestDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; }
        public int? OrganizationId { get; set; }
        public int NationalSocietyId { get; set; }

        public class CreateCoordinatorValidator : AbstractValidator<CreateCoordinatorRequestDto>
        {
            public CreateCoordinatorValidator(IOrganizationService organizationService)
            {
                RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
                RuleFor(m => m.Email).NotEmpty().MaximumLength(100).EmailAddress();
                RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20).PhoneNumber();
                RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20).PhoneNumber().Unless(r => string.IsNullOrEmpty(r.AdditionalPhoneNumber));
                RuleFor(m => m.Organization).MaximumLength(100);
                RuleFor(m => m.OrganizationId)
                    .MustAsync((model, _, t) => organizationService.ValidateAccessForAssigningOrganizationToUser(model.NationalSocietyId))
                    .When(model => model.OrganizationId.HasValue)
                    .WithMessageKey(ResultKey.Organization.NoAccessToChangeOrganization);
            }
        }
    }
}
