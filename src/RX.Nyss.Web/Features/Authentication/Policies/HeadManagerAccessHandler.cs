using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class HeadManagerAccessHandlerRequirement : IAuthorizationRequirement
    {
    }

    public class HeadManagerAccessHandler : AuthorizationHandler<HeadManagerAccessHandlerRequirement>
    {
        private const string RouteParameterName = "nationalSocietyId";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private readonly IEnumerable<string> _rolesWithSameAccessAsHeadManager;

        public HeadManagerAccessHandler(IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;

            // ToDo: Global Coordinator does not have permissions to SMS Gateways, Geographical structure, Projects, etc. but Head Manager does. Investigate and fix later
            _rolesWithSameAccessAsHeadManager = new[] { Role.GlobalCoordinator, Role.Administrator, Role.TechnicalAdvisor }
                .Select(role => role.ToString());
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HeadManagerAccessHandlerRequirement requirement)
        {
            var nationalSocietyId = _httpContextAccessor.GetResourceParameter(RouteParameterName);

            if (!context.User.Identity.IsAuthenticated || !nationalSocietyId.HasValue)
            {
                return;
            }

            var identityName = context.User.Identity.Name;
            var hasSimilarAccessAsHeadManager = context.User.GetRoles().Any(x => _rolesWithSameAccessAsHeadManager.Contains(x));

            if (hasSimilarAccessAsHeadManager || await _userService.IsHeadManagerToNationalSociety(identityName, nationalSocietyId.Value))
            {
                context.Succeed(requirement);
            }
        }
    }
}
