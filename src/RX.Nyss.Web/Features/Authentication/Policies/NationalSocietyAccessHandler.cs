using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Features.NationalSociety;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class NationalSocietyAccessRequirement : IAuthorizationRequirement
    {
    }

    public class NationalSocietyAccessHandler : AuthorizationHandler<NationalSocietyAccessRequirement>
    {
        private const string RouteParameterName = "nationalSocietyId";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INationalSocietyService _nationalSocietyService;

        public NationalSocietyAccessHandler(
            IHttpContextAccessor httpContextAccessor,
            INationalSocietyService nationalSocietyService)
        {
            _httpContextAccessor = httpContextAccessor;
            _nationalSocietyService = nationalSocietyService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, NationalSocietyAccessRequirement requirement)
        {
            var nationalSocietyId = _httpContextAccessor.GetResourceParameter(RouteParameterName);
            if (!context.User.Identity.IsAuthenticated || !nationalSocietyId.HasValue)
            {
                return;
            }

            var roles = context.User.GetRoles();
            var identityName = context.User.Identity.Name;

            if (await _nationalSocietyService.HasUserAccessNationalSocieties(new List<int>{ nationalSocietyId.Value }, identityName, roles))
            {
                context.Succeed(requirement);
            }
        }
    }
}
