using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class DataManagerAccessRequirement : IAuthorizationRequirement
    {
    }

    public class DataManagerAccessHandler : AuthorizationHandler<DataManagerAccessRequirement>
    {
        private const string RouteParameterName = "dataManagerId";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IResourceAccessService _resourceAccessService;

        public DataManagerAccessHandler(IHttpContextAccessor httpContextAccessor, IResourceAccessService resourceAccessService)
        {
            _httpContextAccessor = httpContextAccessor;
            _resourceAccessService = resourceAccessService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DataManagerAccessRequirement requirement)
        {
            var dataManagerId = _httpContextAccessor.GetRouteParameterAsInt(RouteParameterName);
            if (!context.User.Identity.IsAuthenticated || !dataManagerId.HasValue)
            {
                return;
            }

            var dataManagerNationalSocieties = await _resourceAccessService.GetUserNationalSocietyIds<DataManagerUser>(dataManagerId.Value);
            if (await _resourceAccessService.GetUserHasAccessToAnyOfResourceNationalSocieties(context.User, dataManagerNationalSocieties))
            {
                context.Succeed(requirement);
            }
        }
            
    }
}
