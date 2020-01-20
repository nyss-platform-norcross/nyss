using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.NationalSocieties.Access;

namespace RX.Nyss.Web.Features.NationalSocietyStructure.Access
{
    public interface INationalSocietyStructureAccessService
    {
        Task<bool> HasCurrentUserAccessToRegion(int regionId);
        Task<bool> HasCurrentUserAccessToDistrict(int districtId);
        Task<bool> HasCurrentUserAccessToVillage(int villageId);
        Task<bool> HasCurrentUserAccessToZone(int zoneId);
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

        public async Task<bool> HasCurrentUserAccessToRegion(int regionId)
        {
            var nationalSocietyId = await _nyssContext.Regions
                .Where(r => r.Id == regionId)
                .Select(r => r.NationalSociety.Id)
                .FirstOrDefaultAsync();

            return await _nationalSocietyAccessService.HasCurrentUserAccessToNationalSocieties(new[] { nationalSocietyId });
        }

        public async Task<bool> HasCurrentUserAccessToDistrict(int districtId)
        {
            var nationalSocietyId = await _nyssContext.Districts
                .Where(r => r.Id == districtId)
                .Select(r => r.Region.NationalSociety.Id)
                .FirstOrDefaultAsync();

            return await _nationalSocietyAccessService.HasCurrentUserAccessToNationalSocieties(new[] { nationalSocietyId });
        }

        public async Task<bool> HasCurrentUserAccessToVillage(int villageId)
        {
            var nationalSocietyId = await _nyssContext.Villages
                .Where(r => r.Id == villageId)
                .Select(r => r.District.Region.NationalSociety.Id)
                .FirstOrDefaultAsync();

            return await _nationalSocietyAccessService.HasCurrentUserAccessToNationalSocieties(new[] { nationalSocietyId });
        }

        public async Task<bool> HasCurrentUserAccessToZone(int zoneId)
        {
            var nationalSocietyId = await _nyssContext.Zones
                .Where(r => r.Id == zoneId)
                .Select(r => r.Village.District.Region.NationalSociety.Id)
                .FirstOrDefaultAsync();

            return await _nationalSocietyAccessService.HasCurrentUserAccessToNationalSocieties(new[] { nationalSocietyId });
        }
    }
}
