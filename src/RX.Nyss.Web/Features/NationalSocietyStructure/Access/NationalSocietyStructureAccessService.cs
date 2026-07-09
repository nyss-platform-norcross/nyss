using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.NationalSocieties.Access;

namespace RX.Nyss.Web.Features.NationalSocietyStructure.Access
{
    public interface INationalSocietyStructureAccessService
    {
        Task<bool> HasCurrentUserAccessToRegion(int regionId, bool readOnly = false);
        Task<bool> HasCurrentUserAccessToDistrict(int districtId, bool readOnly = false);
        Task<bool> HasCurrentUserAccessToVillage(int villageId, bool readOnly = false);
        Task<bool> HasCurrentUserAccessToZone(int zoneId, bool readOnly = false);
    }

    public class NationalSocietyStructureAccessService : INationalSocietyStructureAccessService
    {
        private readonly INyssContext _nyssContext;
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;

        public NationalSocietyStructureAccessService(
            INyssContext nyssContext,
            INationalSocietyAccessService nationalSocietyAccessService)
        {
            _nyssContext = nyssContext;
            _nationalSocietyAccessService = nationalSocietyAccessService;
        }

        public async Task<bool> HasCurrentUserAccessToRegion(int regionId, bool readOnly)
        {
            var nationalSocietyId = await _nyssContext.Regions
                .Where(r => r.Id == regionId)
                .Select(r => r.NationalSociety.Id)
                .FirstOrDefaultAsync();

            return readOnly
                ? await _nationalSocietyAccessService.HasCurrentUserAccessToNationalSociety(nationalSocietyId)
                : await _nationalSocietyAccessService.HasCurrentUserAccessToModifyNationalSociety(nationalSocietyId);
        }

        public async Task<bool> HasCurrentUserAccessToDistrict(int districtId, bool readOnly)
        {
            var nationalSocietyId = await _nyssContext.Districts
                .Where(r => r.Id == districtId)
                .Select(r => r.Region.NationalSociety.Id)
                .FirstOrDefaultAsync();

            return readOnly
                ? await _nationalSocietyAccessService.HasCurrentUserAccessToNationalSociety(nationalSocietyId)
                : await _nationalSocietyAccessService.HasCurrentUserAccessToModifyNationalSociety(nationalSocietyId);
        }

        public async Task<bool> HasCurrentUserAccessToVillage(int villageId, bool readOnly)
        {
            var nationalSocietyId = await _nyssContext.Villages
                .Where(r => r.Id == villageId)
                .Select(r => r.District.Region.NationalSociety.Id)
                .FirstOrDefaultAsync();

            return readOnly
                ? await _nationalSocietyAccessService.HasCurrentUserAccessToNationalSociety(nationalSocietyId)
                : await _nationalSocietyAccessService.HasCurrentUserAccessToModifyNationalSociety(nationalSocietyId);
        }

        public async Task<bool> HasCurrentUserAccessToZone(int zoneId, bool readOnly)
        {
            var nationalSocietyId = await _nyssContext.Zones
                .Where(r => r.Id == zoneId)
                .Select(r => r.Village.District.Region.NationalSociety.Id)
                .FirstOrDefaultAsync();

            return readOnly
                ? await _nationalSocietyAccessService.HasCurrentUserAccessToNationalSociety(nationalSocietyId)
                : await _nationalSocietyAccessService.HasCurrentUserAccessToModifyNationalSociety(nationalSocietyId);
        }
    }
}
