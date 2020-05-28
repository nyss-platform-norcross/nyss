using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.ProjectOrganizations.Access
{
    public class ProjectOrganizationAccessHandler : ResourceAccessHandler<ProjectOrganizationAccessHandler>
    {
        private readonly IProjectOrganizationAccessService _projectOrganizationAccessService;

        public ProjectOrganizationAccessHandler(IHttpContextAccessor httpContextAccessor, IProjectOrganizationAccessService projectOrganizationAccessService)
            : base("projectOrganizationId", httpContextAccessor)
        {
            _projectOrganizationAccessService = projectOrganizationAccessService;
        }

        protected override Task<bool> HasAccess(int projectOrganizationId, bool readOnly) =>
            _projectOrganizationAccessService.HasCurrentUserAccessToProjectOrganization(projectOrganizationId);
    }
}
