using System.Collections.Generic;
using FluentValidation;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Organizations;
using RX.Nyss.Web.Features.Projects.Access;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.Supervisors.Dto
{
    public class EditSupervisorRequestDto
    {
        public string Name { get; set; }
        public Sex Sex { get; set; }
        public int DecadeOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public int? ProjectId { get; set; }
        public int? OrganizationId { get; set; }
        public string Organization { get; set; }
        public IEnumerable<int> SupervisorAlertRecipients { get; set; }
        public int NationalSocietyId { get; set; }

        public class EditSupervisorRequestValidator : AbstractValidator<EditSupervisorRequestDto>
        {
            public EditSupervisorRequestValidator(IOrganizationService organizationService, IProjectAccessService projectAccessService)
            {
                RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
                RuleFor(m => m.Sex).IsInEnum();
                RuleFor(m => m.DecadeOfBirth).NotEmpty().Must(y => y % 10 == 0).WithMessage(ResultKey.Validation.BirthGroupStartYearMustBeMulipleOf10);
                RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20).PhoneNumber();
                RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20).PhoneNumber().Unless(r => string.IsNullOrEmpty(r.AdditionalPhoneNumber));
                RuleFor(s => s.SupervisorAlertRecipients).NotNull();
                RuleFor(p => p.ProjectId)
                    .MustAsync((projectId, _) => projectAccessService.HasCurrentUserAccessToProject(projectId.Value))
                    .When(m => m.ProjectId.HasValue)
                    .WithMessage(ResultKey.Unauthorized);
            }
        }
    }
}
