using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;
using RX.Nyss.Web.Utils.DataContract;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.NationalSocietyStructure
{
    public interface INationalSocietyStructureService
    {
        Task<Result<List<RegionResponseDto>>> GetRegions(int nationalSocietyId);
        Task<Result<List<DistrictResponseDto>>> GetDistricts(int regionId);
        Task<Result<List<VillageResponseDto>>> GetVillages(int districtId);
        Task<Result<List<ZoneResponseDto>>> GetZones(int villageId);
    }

    public class NationalSocietyStructureService : INationalSocietyStructureService
    {
        private readonly INyssContext _nyssContext;

        public NationalSocietyStructureService(INyssContext context)
        {
            _nyssContext = context;
        }

        public async Task<Result<List<RegionResponseDto>>> GetRegions(int nationalSocietyId)
        {
            var regions = await _nyssContext.Regions
                .Where(r => r.NationalSociety.Id == nationalSocietyId)
                .Select(n => new RegionResponseDto
                {
                    Id = n.Id,
                    Name = n.Name
                })
                .ToListAsync();

            return Success(regions);
        }

        public async Task<Result<List<DistrictResponseDto>>> GetDistricts(int regionId)
        {
            var regions = await _nyssContext.Districts
                .Where(r => r.Region.Id == regionId)
                .Select(n => new DistrictResponseDto
                {
                    Id = n.Id,
                    Name = n.Name
                })
                .ToListAsync();

            return Success(regions);
        }

        public async Task<Result<List<VillageResponseDto>>> GetVillages(int districtId)
        {
            var regions = await _nyssContext.Villages
                .Where(r => r.District.Id == districtId)
                .Select(n => new VillageResponseDto
                {
                    Id = n.Id,
                    Name = n.Name
                })
                .ToListAsync();

            return Success(regions);
        }

        public async Task<Result<List<ZoneResponseDto>>> GetZones(int villageId)
        {
            var regions = await _nyssContext.Zones
                .Where(r => r.Village.Id == villageId)
                .Select(n => new ZoneResponseDto
                {
                    Id = n.Id,
                    Name = n.Name
                })
                .ToListAsync();

            return Success(regions);
        }
    }
}
