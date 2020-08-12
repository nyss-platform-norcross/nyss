using FluentValidation;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Features.ProjectOrganizations.Validation;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.ProjectOrganizations.Dto
{
    public class ProjectOrganizationRequestDto
    {
        public int OrganizationId { get; set; }
        public int ProjectId { get; set; }

        public class ProjectOrganizationValidator : AbstractValidator<ProjectOrganizationRequestDto>
        {
            public ProjectOrganizationValidator(IProjectOrganizationValidationService projectOrganizationValidationService)
            {
                RuleFor(x => x.OrganizationId).NotEmpty();
                RuleFor(po => po.OrganizationId)
                    .MustAsync(async (model, _, t) =>
                        !await projectOrganizationValidationService.OrganizationAlreadyAddedToProject(model.OrganizationId, model.ProjectId))
                    .WithMessageKey(ResultKey.ProjectOrganization.OrganizationAlreadyAdded);
            }
        }
    }
}
