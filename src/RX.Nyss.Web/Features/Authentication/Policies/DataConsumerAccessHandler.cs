using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.NationalSociety;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class DataConsumerAccessRequirement : IAuthorizationRequirement
    {
    }

    public class DataConsumerAccessHandler : AuthorizationHandler<DataConsumerAccessRequirement>
    {
        private const string RouteParameterName = "dataConsumerId";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private readonly INationalSocietyService _nationalSocietyService;

        public DataConsumerAccessHandler(
            IHttpContextAccessor httpContextAccessor,
            IUserService userService,
            INationalSocietyService nationalSocietyService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _nationalSocietyService = nationalSocietyService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            DataConsumerAccessRequirement requirement)
        {
            var dataConsumerId = _httpContextAccessor.GetResourceParameter(RouteParameterName);
            if (!context.User.Identity.IsAuthenticated || !dataConsumerId.HasValue)
            {
                return;
            }

            var dataConsumerNationalSocieties = await _userService.GetUserNationalSocietyIds<DataConsumerUser>(dataConsumerId.Value);
            var roles = context.User.GetRoles();
            var identityName = context.User.Identity.Name;

            if (await _nationalSocietyService.HasUserAccessNationalSocieties(dataConsumerNationalSocieties, identityName, roles))
            {
                context.Succeed(requirement);
            }
        }

        
    }

}
