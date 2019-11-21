using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class ZoneAccessRequirement : IAuthorizationRequirement
    {
    }

    public class ZoneAccessHandler : AuthorizationHandler<ZoneAccessRequirement>
    {
        private const string RouteParameterName = "zoneId";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private readonly INyssContext _nyssContext;

        public ZoneAccessHandler(IHttpContextAccessor httpContextAccessor, IUserService userService, INyssContext nyssContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _nyssContext = nyssContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ZoneAccessRequirement requirement)
        {
            var zoneId = _httpContextAccessor.GetRouteParameterAsInt(RouteParameterName);
            if (!context.User.Identity.IsAuthenticated || !zoneId.HasValue)
            {
                return;
            }

            if (_userService.HasAccessToAllNationalSocieties(context.User.GetRoles()))
            {
                context.Succeed(requirement);
                return;
            }

            var nationalSocietyId = await _nyssContext.Zones
                .Where(r => r.Id == zoneId)
                .Select(r => r.Village.District.Region.NationalSociety.Id)
                .FirstOrDefaultAsync();

            if (await _userService.HasUserAccessToNationalSociety(nationalSocietyId, context.User.Identity.Name))
            {
                context.Succeed(requirement);
            }
        }
    }
}
