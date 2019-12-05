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
    public class VillageAccessRequirement : IAuthorizationRequirement
    {
    }

    public class VillageAccessHandler : AuthorizationHandler<VillageAccessRequirement>
    {
        private const string RouteParameterName = "villageId";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private readonly INyssContext _nyssContext;

        public VillageAccessHandler(IHttpContextAccessor httpContextAccessor, IUserService userService, INyssContext nyssContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _nyssContext = nyssContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, VillageAccessRequirement requirement)
        {
            var villageId = _httpContextAccessor.GetResourceParameter(RouteParameterName);
            if (!context.User.Identity.IsAuthenticated || !villageId.HasValue)
            {
                return;
            }

            if (_userService.HasAccessToAllNationalSocieties(context.User.GetRoles()))
            {
                context.Succeed(requirement);
                return;
            }

            var nationalSocietyId = await _nyssContext.Villages
                .Where(r => r.Id == villageId)
                .Select(r => r.District.Region.NationalSociety.Id)
                .FirstOrDefaultAsync();

            if (await _userService.HasUserAccessToNationalSociety(nationalSocietyId, context.User.Identity.Name))
            {
                context.Succeed(requirement);
            }
        }
    }
}
