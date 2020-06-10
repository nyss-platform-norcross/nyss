using System.Collections.Generic;
using FluentValidation;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Features.Projects.Access;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Projects.Dto
{
    public class CreateProjectRequestDto
    {
        public string Name { get; set; }

        public string TimeZoneId { get; set; }

        public int? OrganizationId { get; set; }

        public IEnumerable<ProjectHealthRiskRequestDto> HealthRisks { get; set; }

        public bool AllowMultipleOrganizations { get; set; }

        public class Validator : AbstractValidator<CreateProjectRequestDto>
        {
            public Validator(IProjectAccessService projectAccessService)
            {
                RuleFor(p => p.Name).NotEmpty().MaximumLength(200);
                RuleFor(p => p.Name).MaximumLength(50);
                RuleFor(p => p.HealthRisks).NotNull();
                RuleForEach(p => p.HealthRisks).SetValidator(new ProjectHealthRiskRequestDto.Validator());

                RuleFor(m => m.AllowMultipleOrganizations)
                    .Must((model, _, t) => projectAccessService.HasCurrentUserAccessToAssignOrganizationToProject())
                    .When(model => model.AllowMultipleOrganizations)
                    .WithMessageKey(ResultKey.Project.NoAccessToSetOrgBasedAccessControl);

                RuleFor(m => m.OrganizationId)
                    .Must((model, _, t) => projectAccessService.HasCurrentUserAccessToAssignOrganizationToProject())
                    .When(model => model.OrganizationId.HasValue)
                    .WithMessageKey(ResultKey.Organization.NoAccessToChangeOrganization);
            }
        }
    }
}
