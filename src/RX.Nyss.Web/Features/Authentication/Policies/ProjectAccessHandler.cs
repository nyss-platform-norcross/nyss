using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Supervisor;
using RX.Nyss.Web.Features.User;
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
        private readonly INyssContext _nyssContext;
        private readonly IUserService _userService;
        private readonly ISupervisorService _supervisorService;

        public ProjectAccessHandler(IHttpContextAccessor httpContextAccessor, INyssContext nyssContext, IUserService userService, ISupervisorService supervisorService)
        {
            _httpContextAccessor = httpContextAccessor;
            _nyssContext = nyssContext;
            _userService = userService;
            _supervisorService = supervisorService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ProjectAccessRequirement requirement)
        {
            var projectId = _httpContextAccessor.GetRouteParameterAsInt(RouteParameterName);
            if (!context.User.Identity.IsAuthenticated || !projectId.HasValue)
            {
                return;
            }

            var roles = context.User.GetRoles();
            var identityName = context.User.Identity.Name;
            
            if (await HasAccessToProject(projectId.Value, roles, identityName))
            { 
                context.Succeed(requirement);
            }
        }

        private async Task<bool> HasAccessToProject(int projectId, IEnumerable<string> roles, string identityName)
        {
            var projectData = _nyssContext.Projects
                .Select(p => new { ProjectId = p.Id, p.NationalSocietyId })
                .Single(p => p.ProjectId == projectId);

            var hasAccessToNationalSociety = await _userService.GetUserHasAccessToAnyOfProvidedNationalSocieties(new List<int> { projectData.NationalSocietyId }, identityName, roles);

            if (!IsSupervisor(roles))
            {
                return hasAccessToNationalSociety;
            }

            var hasAccessToProject = await _supervisorService.GetSupervisorHasAccessToProject(identityName, projectId);
            return hasAccessToNationalSociety && hasAccessToProject;
        }

        private bool IsSupervisor(IEnumerable<string> roles) =>
            roles.Contains(Role.Supervisor.ToString());
    }
}
