using System.Collections.Generic;
using FluentValidation;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Organizations;
using RX.Nyss.Web.Features.Projects.Access;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Supervisors.Dto
{
    public class CreateSupervisorRequestDto
    {
        public string Name { get; set; }
        public Sex Sex { get; set; }
        public int DecadeOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Email { get; set; }
        public int? ProjectId { get; set; }
        public int? OrganizationId { get; set; }
        public int NationalSocietyId { get; set; }
        public string Organization { get; set; }
        public int? HeadSupervisorId { get; set; }
        public int? ModemId { get; set; }

        public class CreateSupervisorValidator : AbstractValidator<CreateSupervisorRequestDto>
        {
            public CreateSupervisorValidator(IOrganizationService organizationService, IProjectAccessService projectAccessService)
            {
                RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
                RuleFor(m => m.Sex).IsInEnum();
                RuleFor(m => m.DecadeOfBirth).NotEmpty().Must(y => y % 10 == 0)
                  .WithMessageKey(ResultKey.Validation.BirthGroupStartYearMustBeMulipleOf10);
                RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20).PhoneNumber();
                RuleFor(m => m.Email).NotEmpty().MaximumLength(100).EmailAddress();
                RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20).PhoneNumber().Unless(r => string.IsNullOrEmpty(r.AdditionalPhoneNumber));
                RuleFor(p => p.ProjectId)
                    .MustAsync((projectId, _) => projectAccessService.HasCurrentUserAccessToProject(projectId.Value))
                    .When(m => m.ProjectId.HasValue)
                    .WithMessageKey(ResultKey.Unauthorized);
                RuleFor(m => m.OrganizationId)
                    .MustAsync((model, _, t) => organizationService.ValidateAccessForAssigningOrganizationToUser(model.NationalSocietyId))
                    .When(model => model.OrganizationId.HasValue)
                    .WithMessageKey(ResultKey.Organization.NoAccessToChangeOrganization);
                RuleFor(m => m.ModemId)
                    .GreaterThan(0)
                    .When(m => m.ModemId.HasValue);
            }
        }
    }
}
