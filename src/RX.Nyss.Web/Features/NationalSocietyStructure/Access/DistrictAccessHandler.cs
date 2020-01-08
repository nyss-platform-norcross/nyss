using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.NationalSocietyStructure.Access
{
    public class DistrictAccessHandler : ResourceAccessHandler<DistrictAccessHandler>
    {
        private readonly INationalSocietyStructureAccessService _nationalSocietyStructureAccessService;

        public DistrictAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyStructureAccessService nationalSocietyStructureAccessService)
            : base("districtId", httpContextAccessor)
        {
            _nationalSocietyStructureAccessService = nationalSocietyStructureAccessService;
        }

        protected override Task<bool> HasAccess(int districtId) =>
            _nationalSocietyStructureAccessService.HasCurrentUserAccessToDistrict(districtId);
    }
}
