using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.Project;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class AlertAccessRequirement : IAuthorizationRequirement
    {
    }

    public class AlertAccessHandler : AuthorizationHandler<AlertAccessRequirement>
    {
        private const string RouteParameterName = "alertId";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INyssContext _nyssContext;
        private readonly IProjectService _projectService;

        public AlertAccessHandler(
            IHttpContextAccessor httpContextAccessor,
            INyssContext nyssContext,
            IProjectService projectService)
        {
            _httpContextAccessor = httpContextAccessor;
            _nyssContext = nyssContext;
            _projectService = projectService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AlertAccessRequirement requirement)
        {
            var alertId = _httpContextAccessor.GetResourceParameter(RouteParameterName);

            if (!context.User.Identity.IsAuthenticated || !alertId.HasValue)
            {
                return;
            }

            var projectId = await _nyssContext.Alerts.Where(a => a.Id == alertId).Select(a => a.ProjectHealthRisk.Project.Id).SingleAsync();

            if (await _projectService.HasCurrentUserAccessToProject(projectId))
            {
                context.Succeed(requirement);
            }
        }
    }
}
