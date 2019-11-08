using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
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

        public DataCollectorAccessHandler(IHttpContextAccessor httpContextAccessor, INyssContext nyssContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _nyssContext = nyssContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            DataCollectorAccessRequirement requirement)
        {
            var dataCollectorId = _httpContextAccessor.GetRouteParameterAsInt(RouteParameterName);
            if (!context.User.Identity.IsAuthenticated || !dataCollectorId.HasValue)
            {
                return;
            }

            var project = await _nyssContext.DataCollectors.Select(dc => dc.Project).FirstOrDefaultAsync(dc => dc.Id == dataCollectorId.Value);
            var roles = context.User.GetRoles();
            var identityName = context.User.Identity.Name;

            context.Succeed(requirement);
            // TODO: check if user has access to project
        }

        
    }

}
