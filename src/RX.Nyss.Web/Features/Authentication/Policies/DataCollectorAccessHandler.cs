using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.DataCollector;
using RX.Nyss.Web.Features.Supervisor;
using RX.Nyss.Web.Features.User;
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
        private readonly IUserService _userService;
        private readonly ISupervisorService _supervisorService;
        private readonly IDataCollectorService _dataCollectorService;

        public DataCollectorAccessHandler(IHttpContextAccessor httpContextAccessor, INyssContext nyssContext, ISupervisorService supervisorService, IUserService userService, IDataCollectorService dataCollectorService)
        {
            _httpContextAccessor = httpContextAccessor;
            _nyssContext = nyssContext;
            _supervisorService = supervisorService;
            _userService = userService;
            _dataCollectorService = dataCollectorService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            DataCollectorAccessRequirement requirement)
        {
            var dataCollectorId = _httpContextAccessor.GetRouteParameterAsInt(RouteParameterName);
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

            var hasAccessToNationalSociety = await _userService.GetUserHasAccessToAnyOfProvidedNationalSocieties(new List<int> { dataCollectorData.NationalSocietyId }, identityName, roles);

            if (!IsSupervisor(roles))
            {
                return hasAccessToNationalSociety;
            }

            var hasAccessToProject = await _supervisorService.GetSupervisorHasAccessToProject(identityName, dataCollectorData.ProjectId);
            var hasAccessToDataCollector = await _dataCollectorService.GetDataCollectorIsSubordinateOfSupervisor(identityName, dataCollectorId);

            return hasAccessToNationalSociety && hasAccessToProject && hasAccessToDataCollector;
        }

        private bool IsSupervisor(IEnumerable<string> roles) =>
            roles.Contains(Role.Supervisor.ToString());


    }

}
