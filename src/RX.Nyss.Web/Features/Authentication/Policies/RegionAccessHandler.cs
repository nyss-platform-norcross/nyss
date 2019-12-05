using System.Collections.Generic;
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
    public class RegionAccessRequirement : IAuthorizationRequirement
    {
    }

    public class RegionAccessHandler : AuthorizationHandler<RegionAccessRequirement>
    {
        private const string RouteParameterName = "regionId";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private readonly INyssContext _nyssContext;

        public RegionAccessHandler(IHttpContextAccessor httpContextAccessor, IUserService userService, INyssContext nyssContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _nyssContext = nyssContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RegionAccessRequirement requirement)
        {
            var regionId = _httpContextAccessor.GetResourceParameter(RouteParameterName);
            if (!context.User.Identity.IsAuthenticated || !regionId.HasValue)
            {
                return;
            }

            if (_userService.HasAccessToAllNationalSocieties(context.User.GetRoles()))
            {
                context.Succeed(requirement);
                return;
            }

            var nationalSocietyId = await _nyssContext.Regions
                .Where(r => r.Id == regionId)
                .Select(r => r.NationalSociety.Id)
                .FirstOrDefaultAsync();

            if (await _userService.HasUserAccessToNationalSociety(nationalSocietyId, context.User.Identity.Name))
            {
                context.Succeed(requirement);
            }
        }
    }
}
