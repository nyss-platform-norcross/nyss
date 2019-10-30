using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class NationalSocietyAccessRequirement : IAuthorizationRequirement
    {
    }

    public class NationalSocietyAccessHandler : AuthorizationHandler<NationalSocietyAccessRequirement>
    {
        private const string RouteValueName = "nationalSocietyId";
        public static readonly ResourceType ResourceType = ResourceType.NationalSociety;

        private readonly IEnumerable<string> _rolesWithAccessToAllNationalSocieties;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public NationalSocietyAccessHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _rolesWithAccessToAllNationalSocieties = new[] { Role.Administrator, Role.GlobalCoordinator }
                .Select(role => role.ToString());
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NationalSocietyAccessRequirement requirement)
        {
            var routeValue = _httpContextAccessor.HttpContext.Request.RouteValues[RouteValueName];
            if (routeValue == null)
            {
                return Task.CompletedTask;
            }

            if (HasAccessToSpecificNationalSociety(context, routeValue.ToString()) || HasAccessToAllNationalSocieties(context))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private bool HasAccessToSpecificNationalSociety(AuthorizationHandlerContext context, string routeValue) => 
            context.User.Claims.Any(c => c.Type == ClaimType.ResourceAccess && c.Value == $"{ResourceType}:{routeValue}");

        private bool HasAccessToAllNationalSocieties(AuthorizationHandlerContext context) => 
            context.User.Claims.Any(c => c.Type == ClaimTypes.Role && _rolesWithAccessToAllNationalSocieties.Contains(c.Value));
    }
}
