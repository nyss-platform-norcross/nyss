using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.Authentication.Policies.BaseAccessHandlers
{
    public abstract class UserAccessHandler<TUser, TRequirement> : RouteBasedAccessHandler<TRequirement> 
        where TUser: Nyss.Data.Models.User
        where TRequirement : IAuthorizationRequirement
    {
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;

        protected UserAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyAccessService nationalSocietyAccessService, string routeParameterName)
            : base(httpContextAccessor, routeParameterName)
        {
            _nationalSocietyAccessService = nationalSocietyAccessService;
        }

        protected async Task HandleUserResourceRequirement(AuthorizationHandlerContext context, TRequirement requirement)
        {
            var resourceId = GetResourceIdFromRoute();
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
