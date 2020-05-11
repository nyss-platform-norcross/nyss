using FluentValidation;

namespace RX.Nyss.Web.Features.ProjectOrganizations.Dto
{
    public class ProjectOrganizationRequestDto
    {
        public int OrganizationId { get; set; }

        public class ProjectOrganizationValidator : AbstractValidator<ProjectOrganizationRequestDto>
        {
            public ProjectOrganizationValidator()
            {
                RuleFor(x => x.OrganizationId).NotEmpty();
            }
        }
    }
}
