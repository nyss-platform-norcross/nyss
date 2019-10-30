using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace RX.Nyss.Web.Utils.AuthorizationHandler
{
    public class AccessToNationalSocietyRequirement : IAuthorizationRequirement
    {
    }

    public class AccessToNationalSocietyHandler : AuthorizationHandler<AccessToNationalSocietyRequirement>
    {
        const string RouteValueName = "nationalSocietyId";
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public AccessToNationalSocietyHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessToNationalSocietyRequirement requirement)
        {
            var routeValue = _httpContextAccessor.HttpContext.Request.RouteValues[RouteValueName];
            if (routeValue == null)
            {
                return Task.CompletedTask;
            }

            var hasAccessToSpecificNationalSociety = context.User.Claims.Any(c => c.Type == ClaimType.NationalSociety.ToString() && c.Value == routeValue.ToString());
            var hasAccessToAllNationalSocieties = context.User.Claims.Any(c => c.Type == ClaimType.AllNationalSocieties.ToString() && c.Value == bool.TrueString);

            if (hasAccessToSpecificNationalSociety || hasAccessToAllNationalSocieties)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
