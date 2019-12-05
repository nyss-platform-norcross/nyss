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
    public class DistrictAccessRequirement : IAuthorizationRequirement
    {
    }

    public class DistrictAccessHandler : AuthorizationHandler<DistrictAccessRequirement>
    {
        private const string RouteParameterName = "districtId";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private readonly INyssContext _nyssContext;

        public DistrictAccessHandler(IHttpContextAccessor httpContextAccessor, IUserService userService, INyssContext nyssContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _nyssContext = nyssContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DistrictAccessRequirement requirement)
        {
            var districtId = _httpContextAccessor.GetResourceParameter(RouteParameterName);
            if (!context.User.Identity.IsAuthenticated || !districtId.HasValue)
            {
                return;
            }

            if (_userService.HasAccessToAllNationalSocieties(context.User.GetRoles()))
            {
                context.Succeed(requirement);
                return;
            }

            var nationalSocietyId = await _nyssContext.Districts
                .Where(r => r.Id == districtId)
                .Select(r => r.Region.NationalSociety.Id)
                .FirstOrDefaultAsync();

            if (await _userService.HasUserAccessToNationalSociety(nationalSocietyId, context.User.Identity.Name))
            {
                context.Succeed(requirement);
            }
        }
    }
}
