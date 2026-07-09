using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Projects.Access
{
    public class ProjectAccessHandler : ResourceAccessHandler<ProjectAccessHandler>
    {
        private readonly IProjectAccessService _projectAccessService;

        public ProjectAccessHandler(IHttpContextAccessor httpContextAccessor, IProjectAccessService projectAccessService)
            : base("projectId", httpContextAccessor)
        {
            _projectAccessService = projectAccessService;
        }

        protected override Task<bool> HasAccess(int projectId, bool readOnly) =>
            _projectAccessService.HasCurrentUserAccessToProject(projectId);
    }
}
