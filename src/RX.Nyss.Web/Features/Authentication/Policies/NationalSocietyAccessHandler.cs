using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class NationalSocietyAccessRequirement : IAuthorizationRequirement
    {
    }

    public class NationalSocietyAccessHandler : AuthorizationHandler<NationalSocietyAccessRequirement>
    {
        private const string RouteParameterName = "nationalSocietyId";
        public static readonly ResourceType ResourceType = ResourceType.NationalSociety;

        private readonly IEnumerable<string> _rolesWithAccessToAllNationalSocieties;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;

        public NationalSocietyAccessHandler(IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _rolesWithAccessToAllNationalSocieties = new[] { Role.Administrator, Role.GlobalCoordinator }
                .Select(role => role.ToString());
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

            if (await _userService.GetUserHasAccessToAnyOfProvidedNationalSocieties(new List<int>{ nationalSocietyId.Value }, identityName, roles))
            {
                context.Succeed(requirement);
            }
        }
    }
}
