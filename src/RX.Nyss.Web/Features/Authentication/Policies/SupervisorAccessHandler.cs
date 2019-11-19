using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class SupervisorAccessRequirement : IAuthorizationRequirement
    {
    }

    public class SupervisorAccessHandler : AuthorizationHandler<SupervisorAccessRequirement>
    {
        private const string RouteParameterName = "supervisorId";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;

        public SupervisorAccessHandler(IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SupervisorAccessRequirement requirement)
        {
            var supervisorId = _httpContextAccessor.GetRouteParameterAsInt(RouteParameterName);
            if (!context.User.Identity.IsAuthenticated || !supervisorId.HasValue)
            {
                return;
            }

            var supervisorNationalSocieties = await _userService.GetUserNationalSocietyIds<SupervisorUser>(supervisorId.Value);
            var roles = context.User.GetRoles();
            var identityName = context.User.Identity.Name;

            if (await _userService.GetUserHasAccessToAnyOfProvidedNationalSocieties(supervisorNationalSocieties, identityName, roles))
            {
                context.Succeed(requirement);
            }
        }
            
    }
}
