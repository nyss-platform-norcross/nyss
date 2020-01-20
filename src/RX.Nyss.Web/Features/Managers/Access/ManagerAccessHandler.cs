using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Features.NationalSocieties.Access;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Managers.Access
{
    public class ManagerAccessHandler : ResourceAccessHandler<ManagerAccessHandler>
    {
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;

        public ManagerAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyAccessService nationalSocietyAccessService)
            : base("managerId", httpContextAccessor)
        {
            _nationalSocietyAccessService = nationalSocietyAccessService;
        }

        protected override Task<bool> HasAccess(int managerId) =>
            _nationalSocietyAccessService.HasCurrentUserAccessToUserNationalSocieties(managerId);
    }
}
