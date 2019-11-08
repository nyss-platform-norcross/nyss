using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class ManagerAccessRequirement : IAuthorizationRequirement
    {
    }

    public class ManagerAccessHandler : AuthorizationHandler<ManagerAccessRequirement>
    {
        private const string RouteParameterName = "dataManagerId";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;

        public ManagerAccessHandler(IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ManagerAccessRequirement requirement)
        {
            var dataManagerId = _httpContextAccessor.GetRouteParameterAsInt(RouteParameterName);
            if (!context.User.Identity.IsAuthenticated || !dataManagerId.HasValue)
            {
                return;
            }

            var dataManagerNationalSocieties = await _userService.GetUserNationalSocietyIds<ManagerUser>(dataManagerId.Value);
            var roles = context.User.GetRoles();
            var identityName = context.User.Identity.Name;

            if (await _userService.GetUserHasAccessToAnyOfResourceNationalSocieties(dataManagerNationalSocieties, identityName, roles))
            {
                context.Succeed(requirement);
            }
        }
            
    }
}
