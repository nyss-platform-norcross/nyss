using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.NationalSocietyStructure.Access
{
    public class ZoneAccessHandler : ResourceAccessHandler<ZoneAccessHandler>
    {
        private readonly INationalSocietyStructureAccessService _nationalSocietyStructureAccessService;

        public ZoneAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyStructureAccessService nationalSocietyStructureAccessService)
            : base("zoneId", httpContextAccessor)
        {
            _nationalSocietyStructureAccessService = nationalSocietyStructureAccessService;
        }

        protected override Task<bool> HasAccess(int zoneId) =>
            _nationalSocietyStructureAccessService.HasCurrentUserAccessToZone(zoneId);
    }
}
