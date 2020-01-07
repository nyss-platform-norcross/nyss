using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.NationalSocietyStructure.Access
{
    public class RegionAccessHandler : ResourceAccessHandler<RegionAccessHandler>
    {
        private readonly INationalSocietyStructureAccessService _nationalSocietyStructureAccessService;

        public RegionAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyStructureAccessService nationalSocietyStructureAccessService)
            : base("regionId", httpContextAccessor)
        {
            _nationalSocietyStructureAccessService = nationalSocietyStructureAccessService;
        }

        protected override Task<bool> HasAccess(int regionId) =>
            _nationalSocietyStructureAccessService.HasCurrentUserAccessToRegion(regionId);
    }
}
