using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.DataCollector;
using RX.Nyss.Web.Features.NationalSociety;
using RX.Nyss.Web.Features.Project;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class DataCollectorAccessRequirement : IAuthorizationRequirement
    {
    }

    public class DataCollectorAccessHandler : AuthorizationHandler<DataCollectorAccessRequirement>
    {
        private const string RouteParameterName = "dataCollectorId";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INyssContext _nyssContext;
        private readonly INationalSocietyService _nationalSocietyService;
        private readonly IProjectService _projectService;
        private readonly IDataCollectorService _dataCollectorService;

        public DataCollectorAccessHandler(
            IHttpContextAccessor httpContextAccessor,
            INyssContext nyssContext,
            INationalSocietyService nationalSocietyService,
            IProjectService projectService,
            IDataCollectorService dataCollectorService)
        {
            _httpContextAccessor = httpContextAccessor;
            _nyssContext = nyssContext;
            _nationalSocietyService = nationalSocietyService;
            _projectService = projectService;
            _dataCollectorService = dataCollectorService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            DataCollectorAccessRequirement requirement)
        {
            var dataCollectorId = _httpContextAccessor.GetResourceParameter(RouteParameterName);
            if (!context.User.Identity.IsAuthenticated || !dataCollectorId.HasValue)
            {
                return;
            }

            var roles = context.User.GetRoles();
            var identityName = context.User.Identity.Name;


            if (await HasAccessToDataCollector(dataCollectorId.Value, roles, identityName))
            {
                context.Succeed(requirement);
            }
        }

        private async Task<bool> HasAccessToDataCollector(int dataCollectorId, IEnumerable<string> roles, string identityName)
        {
            var dataCollectorData = await _nyssContext.DataCollectors
                .Select(dc => new { dc.Id, ProjectId = dc.Project.Id, dc.Project.NationalSocietyId })
                .SingleAsync(dc => dc.Id == dataCollectorId);

            var hasAccessToNationalSociety = await _nationalSocietyService.HasUserAccessNationalSocieties(new List<int> { dataCollectorData.NationalSocietyId }, identityName, roles);

            if (!IsSupervisor(roles))
            {
                return hasAccessToNationalSociety;
            }

            var hasAccessToProject = await _projectService.HasSupervisorAccessToProject(identityName, dataCollectorData.ProjectId);
            var hasAccessToDataCollector = await _dataCollectorService.GetDataCollectorIsSubordinateOfSupervisor(identityName, dataCollectorId);

            return hasAccessToNationalSociety && hasAccessToProject && hasAccessToDataCollector;
        }

        private bool IsSupervisor(IEnumerable<string> roles) =>
            roles.Contains(Role.Supervisor.ToString());


    }

}
