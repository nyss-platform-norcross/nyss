using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class TechnicalAdvisorAccessRequirement : IAuthorizationRequirement
    {
    }
    public class TechnicalAdvisorAccessHandler: AuthorizationHandler<TechnicalAdvisorAccessRequirement>
    {
        private const string RouteParameterName = "technicalAdvisorId";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;

        public TechnicalAdvisorAccessHandler(IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TechnicalAdvisorAccessRequirement requirement)
        {
            var technicalAdvisorId = _httpContextAccessor.GetRouteParameterAsInt(RouteParameterName);
            if (!context.User.Identity.IsAuthenticated || !technicalAdvisorId.HasValue)
            {
                return;
            }

            var technicalAdvisorNationalSocieties = await _userService.GetUserNationalSocietyIds<TechnicalAdvisorUser>(technicalAdvisorId.Value);
            var roles = context.User.GetRoles();
            var identityName = context.User.Identity.Name;

            if (await _userService.GetUserHasAccessToAnyOfResourceNationalSocieties(technicalAdvisorNationalSocieties, identityName, roles))
            {
                context.Succeed(requirement);
            }
        }
    }
}
