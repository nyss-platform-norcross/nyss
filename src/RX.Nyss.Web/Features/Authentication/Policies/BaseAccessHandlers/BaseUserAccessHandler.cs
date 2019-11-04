using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.Authentication.Policies.BaseAccessHandlers
{
    public abstract class BaseUserAccessHandler<TUser, TRequirement>: AuthorizationHandler<TRequirement>
        where TUser: Nyss.Data.Models.User
        where TRequirement : IAuthorizationRequirement
    {
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _routeParameterName;

        protected BaseUserAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyAccessService nationalSocietyAccessService, string routeParameterName)
        {
            _httpContextAccessor = httpContextAccessor;
            _nationalSocietyAccessService = nationalSocietyAccessService;
            _routeParameterName = routeParameterName;
        }

        protected async Task HandleUserResourceRequirement(AuthorizationHandlerContext context, TRequirement requirement)
        {
            var resourceId = _httpContextAccessor.GetRouteParameterAsInt(_routeParameterName);
            if (!resourceId.HasValue)
            {
                return;
            }

            var resourceNationalSocietyIds =
                await _nationalSocietyAccessService.GetUserNationalSocietyIds<TUser>(resourceId.Value);
            if (await _nationalSocietyAccessService.HasAccessToAnyOfResourceNationalSocieties(context.User, resourceNationalSocietyIds))
            {
                context.Succeed(requirement);
            }
        }
    }
}
