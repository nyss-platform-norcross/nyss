using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.NationalSocietyStructure.Access
{
    public class VillageAccessHandler : ResourceAccessHandler<VillageAccessHandler>
    {
        private readonly INationalSocietyStructureAccessService _nationalSocietyStructureAccessService;

        public VillageAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyStructureAccessService nationalSocietyStructureAccessService)
            : base("villageId", httpContextAccessor)
        {
            _nationalSocietyStructureAccessService = nationalSocietyStructureAccessService;
        }

        protected override Task<bool> HasAccess(int villageId, bool readOnly) =>
            _nationalSocietyStructureAccessService.HasCurrentUserAccessToVillage(villageId, readOnly);
    }
}
