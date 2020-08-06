using FluentValidation;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Features.ProjectOrganizations.Validation;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.ProjectOrganizations.Dto
{
    public class ProjectOrganizationRequestDto
    {
        public int OrganizationId { get; set; }

        public class ProjectOrganizationValidator : AbstractValidator<ProjectOrganizationRequestDto>
        {
            private const string ProjectIdRouteParameterName = "projectId";

            public ProjectOrganizationValidator(IProjectOrganizationValidationService projectOrganizationValidationService, IHttpContextAccessor httpContextAccessor)
            {
                RuleFor(x => x.OrganizationId).NotEmpty();
                RuleFor(po => po.OrganizationId)
                    .MustAsync(async (model, _, t) =>
                        !await projectOrganizationValidationService.OrganizationAlreadyAddedToProject(model.OrganizationId, httpContextAccessor.GetResourceParameter(ProjectIdRouteParameterName)))
                    .WithMessageKey(ResultKey.ProjectOrganization.OrganizationAlreadyAdded);
            }
        }
    }
}
