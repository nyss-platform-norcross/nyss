using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class TechnicalAdvisorAccessRequirement : IAuthorizationRequirement
    {
    }
    public class TechnicalAdvisorAccessHandler: AuthorizationHandler<TechnicalAdvisorAccessRequirement>
    {
        private const string RouteParameterName = "technicalAdvisorId";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IResourceAccessService _resourceAccessService;

        public TechnicalAdvisorAccessHandler(IHttpContextAccessor httpContextAccessor, IResourceAccessService resourceAccessService)
        {
            _httpContextAccessor = httpContextAccessor;
            _resourceAccessService = resourceAccessService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TechnicalAdvisorAccessRequirement requirement)
        {
            var technicalAdvisorId = _httpContextAccessor.GetRouteParameterAsInt(RouteParameterName);
            if (!context.User.Identity.IsAuthenticated || !technicalAdvisorId.HasValue)
            {
                return;
            }

            var technicalAdvisorNationalSocieties = await _resourceAccessService.GetUserNationalSocietyIds<TechnicalAdvisorUser>(technicalAdvisorId.Value);
            if (await _resourceAccessService.GetUserHasAccessToAnyOfResourceNationalSocieties(context.User, technicalAdvisorNationalSocieties))
            {
                context.Succeed(requirement);
            }
        }
    }
}
