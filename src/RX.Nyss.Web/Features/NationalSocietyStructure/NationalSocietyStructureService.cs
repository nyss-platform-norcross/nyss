using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.NationalSocietyStructure
{
    public interface INationalSocietyStructureService
    {
        Task<Result<StructureResponseDto>> Get(int nationalSocietyId);
        Task<Result<StructureResponseDto.StructureRegionDto>> CreateRegion(int nationalSocietyId, string name);
        Task<Result> EditRegion(int regionId, string name);
        Task<Result> DeleteRegion(int regionId);
        Task<Result<StructureResponseDto.StructureDistrictDto>> CreateDistrict(int regionId, string name);
        Task<Result> EditDistrict(int districtId, string name);
        Task<Result> DeleteDistrict(int districtId);
        Task<Result<StructureResponseDto.StructureVillageDto>> CreateVillage(int districtId, string name);
        Task<Result> EditVillage(int villageId, string name);
        Task<Result> DeleteVillage(int villageId);
        Task<Result<StructureResponseDto.StructureZoneDto>> CreateZone(int villageId, string name);
        Task<Result> EditZone(int zoneId, string name);
        Task<Result> DeleteZone(int zoneId);
        Task<Result<List<RegionResponseDto>>> ListRegions(int nationalSocietyId);
        Task<Result<List<DistrictResponseDto>>> ListDistricts(int regionId);
        Task<Result<List<VillageResponseDto>>> ListVillages(int districtId);
        Task<Result<List<ZoneResponseDto>>> ListZones(int villageId);
    }

    public class NationalSocietyStructureService : INationalSocietyStructureService
    {
        private readonly INyssContext _nyssContext;

        public NationalSocietyStructureService(INyssContext context)
        {
            _nyssContext = context;
        }

        public async Task<Result<StructureResponseDto>> Get(int nationalSocietyId)
        {
            var structure = new StructureResponseDto
            {
                Regions = await _nyssContext.Regions
                    .Where(r => r.NationalSociety.Id == nationalSocietyId)
                    .Select(region => new StructureResponseDto.StructureRegionDto
                    {
                        Id = region.Id,
                        Name = region.Name,
                        Districts = region.Districts.Select(district => new StructureResponseDto.StructureDistrictDto
                        {
                            Id = district.Id,
                            RegionId = region.Id,
                            Name = district.Name,
                            Villages = district.Villages.Select(village => new StructureResponseDto.StructureVillageDto
                            {
                                Id = village.Id,
                                DistrictId = district.Id,
                                Name = village.Name,
                                Zones = village.Zones.Select(zone => new StructureResponseDto.StructureZoneDto
                                {
                                    Id = zone.Id,
                                    VillageId = village.Id,
                                    Name = zone.Name
                                })
                            })
                        })
                    })
                    .ToListAsync()
            };

            return Success(structure);
        }

        public async Task<Result<StructureResponseDto.StructureRegionDto>> CreateRegion(int nationalSocietyId, string name)
        {
            if (await _nyssContext.NationalSocieties.AnyAsync(ns => ns.Id == nationalSocietyId && ns.IsArchived))
            {
                return Error<StructureResponseDto.StructureRegionDto>(ResultKey.NationalSociety.Structure.CannotCreateItemInArchivedNationalSociety);
            }

            if (await _nyssContext.Regions.AnyAsync(ns => ns.Name == name && ns.NationalSociety.Id == nationalSocietyId))
            {
                return Error<StructureResponseDto.StructureRegionDto>(ResultKey.NationalSociety.Structure.ItemAlreadyExists);
            }

            var entry = await _nyssContext.Regions.AddAsync(new Region
            {
                NationalSociety = _nyssContext.NationalSocieties
                    .Attach(new NationalSociety { Id = nationalSocietyId })
                    .Entity,
                Name = name
            });

            await _nyssContext.SaveChangesAsync();

            return Success(new StructureResponseDto.StructureRegionDto
            {
                Id = entry.Entity.Id,
                Name = name,
                Districts = new StructureResponseDto.StructureDistrictDto[0]
            });
        }

        public async Task<Result> EditRegion(int regionId, string name)
        {
            if (await _nyssContext.Regions.AnyAsync(r => r.Name == name && r.Id != regionId))
            {
                return Error<StructureResponseDto.StructureRegionDto>(ResultKey.NationalSociety.Structure.ItemAlreadyExists);
            }

            var entity = await _nyssContext.Regions
                .Where(r => !r.NationalSociety.IsArchived)
                .SingleAsync(r => r.Id == regionId);
            entity.Name = name;
            await _nyssContext.SaveChangesAsync();
            return Success();
        }

        public async Task<Result> DeleteRegion(int regionId)
        {
            var entity = await _nyssContext.Regions
                .Include(r => r.Districts).ThenInclude(d => d.Villages).ThenInclude(v => v.Zones)
                .Where(r => !r.NationalSociety.IsArchived)
                .SingleAsync(region => region.Id == regionId);

            _nyssContext.Regions.Remove(entity);
            await _nyssContext.SaveChangesAsync();
            return Success();
        }

        public async Task<Result<StructureResponseDto.StructureDistrictDto>> CreateDistrict(int regionId, string name)
        {
            if (await _nyssContext.Regions.AnyAsync(r => r.Id == regionId && r.NationalSociety.IsArchived))
            {
                return Error<StructureResponseDto.StructureDistrictDto>(ResultKey.NationalSociety.Structure.CannotCreateItemInArchivedNationalSociety);
            }

            if (await _nyssContext.Districts.AnyAsync(d => d.Name == name && d.Region.Id == regionId))
            {
                return Error<StructureResponseDto.StructureDistrictDto>(ResultKey.NationalSociety.Structure.ItemAlreadyExists);
            }

            var entry = await _nyssContext.Districts.AddAsync(new District
            {
                Region = _nyssContext.Regions.Attach(new Region { Id = regionId }).Entity,
                Name = name
            });

            await _nyssContext.SaveChangesAsync();

            return Success(new StructureResponseDto.StructureDistrictDto
            {
                Id = entry.Entity.Id,
                RegionId = regionId,
                Name = name
            });
        }

        public async Task<Result> EditDistrict(int districtId, string name)
        {
            if (await _nyssContext.Districts.AnyAsync(district => district.Name == name && district.Id != districtId))
            {
                return Error<StructureResponseDto.StructureRegionDto>(ResultKey.NationalSociety.Structure.ItemAlreadyExists);
            }

            var entity = await _nyssContext.Districts
                .Where(d => !d.Region.NationalSociety.IsArchived)
                .SingleAsync(district => district.Id == districtId);
            entity.Name = name;
            await _nyssContext.SaveChangesAsync();
            return Success();
        }

        public async Task<Result> DeleteDistrict(int districtId)
        {
            var entity = await _nyssContext.Districts
                .Include(d => d.Villages).ThenInclude(v => v.Zones)
                .Where(r => !r.Region.NationalSociety.IsArchived)
                .SingleAsync(district => district.Id == districtId);
            _nyssContext.Districts.Remove(entity);
            await _nyssContext.SaveChangesAsync();
            return Success();
        }

        public async Task<Result<StructureResponseDto.StructureVillageDto>> CreateVillage(int districtId, string name)
        {
            if (await _nyssContext.Districts.AnyAsync(d => d.Id == districtId && d.Region.NationalSociety.IsArchived))
            {
                return Error<StructureResponseDto.StructureVillageDto>(ResultKey.NationalSociety.Structure.CannotCreateItemInArchivedNationalSociety);
            }

            if (await _nyssContext.Villages.AnyAsync(d => d.Name == name && d.District.Id == districtId))
            {
                return Error<StructureResponseDto.StructureVillageDto>(ResultKey.NationalSociety.Structure.ItemAlreadyExists);
            }

            var entry = await _nyssContext.Villages.AddAsync(new Village
            {
                District = _nyssContext.Districts.Attach(new District { Id = districtId }).Entity,
                Name = name
            });

            await _nyssContext.SaveChangesAsync();

            return Success(new StructureResponseDto.StructureVillageDto
            {
                Id = entry.Entity.Id,
                DistrictId = districtId,
                Name = name
            });
        }

        public async Task<Result> EditVillage(int villageId, string name)
        {
            if (await _nyssContext.Villages.AnyAsync(village => village.Name == name && village.Id != villageId))
            {
                return Error<StructureResponseDto.StructureRegionDto>(ResultKey.NationalSociety.Structure.ItemAlreadyExists);
            }

            var entity = await _nyssContext.Villages
                .Where(v => !v.District.Region.NationalSociety.IsArchived)
                .SingleAsync(village => village.Id == villageId);
            entity.Name = name;
            await _nyssContext.SaveChangesAsync();
            return Success();
        }

        public async Task<Result> DeleteVillage(int villageId)
        {
            var entity = await _nyssContext.Villages
                .Include(v => v.Zones)
                .Where(v => !v.District.Region.NationalSociety.IsArchived)
                .SingleAsync(village => village.Id == villageId);
            _nyssContext.Villages.Remove(entity);
            await _nyssContext.SaveChangesAsync();
            return Success();
        }

        public async Task<Result<StructureResponseDto.StructureZoneDto>> CreateZone(int villageId, string name)
        {
            if (await _nyssContext.Villages.AnyAsync(v => v.Id == villageId && v.District.Region.NationalSociety.IsArchived))
            {
                return Error<StructureResponseDto.StructureZoneDto>(ResultKey.NationalSociety.Structure.CannotCreateItemInArchivedNationalSociety);
            }
            if (await _nyssContext.Zones.AnyAsync(d => d.Name == name && d.Village.Id == villageId))
            {
                return Error<StructureResponseDto.StructureZoneDto>(ResultKey.NationalSociety.Structure.ItemAlreadyExists);
            }

            var entry = await _nyssContext.Zones.AddAsync(new Zone
            {
                Village = _nyssContext.Villages.Attach(new Village { Id = villageId }).Entity,
                Name = name
            });

            await _nyssContext.SaveChangesAsync();

            return Success(new StructureResponseDto.StructureZoneDto
            {
                Id = entry.Entity.Id,
                VillageId = villageId,
                Name = name
            });
        }

        public async Task<Result> EditZone(int zoneId, string name)
        {
            if (await _nyssContext.Zones.AnyAsync(zone => zone.Name == name && zone.Id != zoneId))
            {
                return Error<StructureResponseDto.StructureVillageDto>(ResultKey.NationalSociety.Structure.ItemAlreadyExists);
            }

            var entity = await _nyssContext.Zones
                .Where(r => !r.Village.District.Region.NationalSociety.IsArchived)
                .SingleAsync(zone => zone.Id == zoneId);
            entity.Name = name;
            await _nyssContext.SaveChangesAsync();
            return Success();
        }

        public async Task<Result> DeleteZone(int zoneId)
        {
            var entity = await _nyssContext.Zones
                .Where(r => !r.Village.District.Region.NationalSociety.IsArchived)
                .SingleAsync(zone => zone.Id == zoneId);
            _nyssContext.Zones.Remove(entity);
            await _nyssContext.SaveChangesAsync();
            return Success();
        }

        public async Task<Result<List<RegionResponseDto>>> ListRegions(int nationalSocietyId)
        {
            var regions = await _nyssContext.Regions
                .Where(r => r.NationalSociety.Id == nationalSocietyId)
                .Select(n => new RegionResponseDto { Id = n.Id, Name = n.Name })
                .ToListAsync();
            return Success(regions);
        }

        public async Task<Result<List<DistrictResponseDto>>> ListDistricts(int regionId)
        {
            var regions = await _nyssContext.Districts
                .Where(r => r.Region.Id == regionId)
                .Select(n => new DistrictResponseDto { Id = n.Id, Name = n.Name })
                .ToListAsync();
            return Success(regions);
        }

        public async Task<Result<List<VillageResponseDto>>> ListVillages(int districtId)
        {
            var regions = await _nyssContext.Villages
                .Where(r => r.District.Id == districtId)
                .Select(n => new VillageResponseDto { Id = n.Id, Name = n.Name })
                .ToListAsync();
            return Success(regions);
        }

        public async Task<Result<List<ZoneResponseDto>>> ListZones(int villageId)
        {
            var regions = await _nyssContext.Zones
                .Where(r => r.Village.Id == villageId)
                .Select(n => new ZoneResponseDto { Id = n.Id, Name = n.Name })
                .ToListAsync();
            return Success(regions);
        }
    }
}
