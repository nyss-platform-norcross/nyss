using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Features.Project;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class ProjectAccessRequirement : IAuthorizationRequirement
    {
    }

    public class ProjectAccessHandler : AuthorizationHandler<ProjectAccessRequirement>
    {
        private const string RouteParameterName = "projectId";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProjectService _projectService;

        public ProjectAccessHandler(
            IHttpContextAccessor httpContextAccessor,
            IProjectService projectService)
        {
            _httpContextAccessor = httpContextAccessor;
            _projectService = projectService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ProjectAccessRequirement requirement)
        {
            var projectId = _httpContextAccessor.GetResourceParameter(RouteParameterName);

            if (!context.User.Identity.IsAuthenticated || !projectId.HasValue)
            {
                return;
            }

            if (await _projectService.HasCurrentUserAccessToProject(projectId.Value))
            { 
                context.Succeed(requirement);
            }
        }
    }
}
