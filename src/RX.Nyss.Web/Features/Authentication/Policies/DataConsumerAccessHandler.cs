using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class DataConsumerAccessRequirement : IAuthorizationRequirement
    {
    }

    public class DataConsumerAccessHandler : AuthorizationHandler<DataConsumerAccessRequirement>
    {
        private const string RouteParameterName = "dataConsumerId";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IResourceAccessService _resourceAccessService;

        public DataConsumerAccessHandler(IHttpContextAccessor httpContextAccessor, IResourceAccessService resourceAccessService)
        {
            _httpContextAccessor = httpContextAccessor;
            _resourceAccessService = resourceAccessService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            DataConsumerAccessRequirement requirement)
        {
            var dataConsumerId = _httpContextAccessor.GetRouteParameterAsInt(RouteParameterName);
            if (!context.User.Identity.IsAuthenticated || !dataConsumerId.HasValue)
            {
                return;
            }

            var dataConsumerNationalSocieties = await _resourceAccessService.GetUserNationalSocietyIds<DataConsumerUser>(dataConsumerId.Value);
            if (await _resourceAccessService.GetUserHasAccessToAnyOfResourceNationalSocieties(context.User, dataConsumerNationalSocieties))
            {
                context.Succeed(requirement);
            }
        }

        
    }

}
