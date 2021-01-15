using System.Collections.Generic;
using FluentValidation;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Organizations;
using RX.Nyss.Web.Features.Projects.Access;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.HeadSupervisors.Dto
{
    public class EditHeadSupervisorRequestDto
    {
        public string Name { get; set; }
        public Sex Sex { get; set; }
        public int DecadeOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public int? ProjectId { get; set; }
        public int? OrganizationId { get; set; }
        public string Organization { get; set; }
        public int NationalSocietyId { get; set; }

        public class EditHeadSupervisorRequestValidator : AbstractValidator<EditHeadSupervisorRequestDto>
        {
            public EditHeadSupervisorRequestValidator(IProjectAccessService projectAccessService)
            {
                RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
                RuleFor(m => m.Sex).IsInEnum();
                RuleFor(m => m.DecadeOfBirth).NotEmpty().Must(y => y % 10 == 0).WithMessageKey(ResultKey.Validation.BirthGroupStartYearMustBeMulipleOf10);
                RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20).PhoneNumber();
                RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20).PhoneNumber().Unless(r => string.IsNullOrEmpty(r.AdditionalPhoneNumber));
                RuleFor(p => p.ProjectId)
                    .MustAsync((projectId, _) => projectAccessService.HasCurrentUserAccessToProject(projectId.Value))
                    .When(m => m.ProjectId.HasValue)
                    .WithMessageKey(ResultKey.Unauthorized);
            }
        }
    }
}
