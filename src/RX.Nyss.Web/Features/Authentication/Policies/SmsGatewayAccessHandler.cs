using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class SmsGatewayAccessRequirement : IAuthorizationRequirement
    {
    }

    public class SmsGatewayAccessHandler : AuthorizationHandler<SmsGatewayAccessRequirement>
    {
        public const string RouteValueName = "smsGatewayId";
        public static readonly ResourceType ResourceType = ResourceType.NationalSociety;
        public static readonly IEnumerable<string> RolesWithAccessToAllSmsGateways = new[] {Role.Administrator.ToString(), Role.GlobalCoordinator.ToString()}.ToList();

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INyssContext _nyssContext;
        
        public SmsGatewayAccessHandler(IHttpContextAccessor httpContextAccessor, INyssContext nyssContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _nyssContext = nyssContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SmsGatewayAccessRequirement requirement)
        {
            var routeValue = _httpContextAccessor.HttpContext.Request.RouteValues[RouteValueName];

            if (routeValue == null || !int.TryParse(routeValue.ToString(), out var smsGatewayId))
            {
                return;
            }

            if (HasAccessToAllSmsGateways(context) || await HasAccessToSpecificNationalSociety(context, smsGatewayId))
            {
                context.Succeed(requirement);
            }
        }

        private async Task<bool> HasAccessToSpecificNationalSociety(AuthorizationHandlerContext context, int smsGatewayId)
        {
            var smsGateway = await _nyssContext.GatewaySettings.FindAsync(smsGatewayId);

            if (smsGateway == null)
            {
                return false;
            }

            return context.User.Claims.Any(c => c.Type == ClaimType.ResourceAccess && c.Value == $"{ResourceType}:{smsGateway.NationalSocietyId}");
        }
            
        private bool HasAccessToAllSmsGateways(AuthorizationHandlerContext context) =>
            context.User.Claims.Any(c => c.Type == ClaimTypes.Role && RolesWithAccessToAllSmsGateways.Contains(c.Value));
    }
}
