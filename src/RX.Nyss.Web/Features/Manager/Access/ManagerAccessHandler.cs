using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Features.NationalSociety.Access;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Manager.Access
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
