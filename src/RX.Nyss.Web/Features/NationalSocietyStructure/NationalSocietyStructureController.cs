using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.NationalSocietyStructure
{
    [Route("api/nationalSocietyStructure")]
    public class NationalSocietyStructureController : BaseController
    {
        private readonly INationalSocietyStructureService _nationalSocietyStructureService;

        public NationalSocietyStructureController(INationalSocietyStructureService nationalSocietyStructureService)
        {
            _nationalSocietyStructureService = nationalSocietyStructureService;
        }

        /// <summary>
        /// Gets a National Society structure
        /// </summary>
        [Route("get"), HttpGet]
        [NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result<StructureResponseDto>> Get(int nationalSocietyId) =>
            await _nationalSocietyStructureService.Get(nationalSocietyId);

        /// <summary>
        /// Create a region to National Society's structure
        /// </summary>
        [Route("region/create"), HttpPost]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result<StructureResponseDto.StructureRegionDto>> CreateRegion(int nationalSocietyId, [FromBody]StructureEntryRequestDto dto) =>
            await _nationalSocietyStructureService.CreateRegion(nationalSocietyId, dto.Name);


        /// <summary>
        /// Updates a region to National Society's structure
        /// </summary>
        [Route("region/{regionId}/edit"), HttpPost]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.RegionAccess)]
        public async Task<Result> EditRegion(int regionId, [FromBody]StructureEntryRequestDto dto) =>
            await _nationalSocietyStructureService.EditRegion(regionId, dto.Name);

        /// <summary>
        /// Deletes a region from the National Society's structure
        /// </summary>
        [Route("region/{regionId}/delete"), HttpPost]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.RegionAccess)]
        public async Task<Result> DeleteRegion(int regionId) =>
            await _nationalSocietyStructureService.DeleteRegion(regionId);

        /// <summary>
        /// Create a district to National Society's structure
        /// </summary>
        [Route("region/{regionId:int}/district/create"), HttpPost]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.RegionAccess)]
        public async Task<Result<StructureResponseDto.StructureDistrictDto>> CreateDistrict(int regionId, [FromBody]StructureEntryRequestDto dto) =>
            await _nationalSocietyStructureService.CreateDistrict(regionId, dto.Name);


        /// <summary>
        /// Updates a district to National Society's structure
        /// </summary>
        [Route("district/{districtId}/edit"), HttpPost]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.DistrictAccess)]
        public async Task<Result> EditDistrict(int districtId, [FromBody]StructureEntryRequestDto dto) =>
            await _nationalSocietyStructureService.EditDistrict(districtId, dto.Name);

        /// <summary>
        /// Deletes a district from the National Society's structure
        /// </summary>
        [Route("district/{districtId}/delete"), HttpPost]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.DistrictAccess)]
        public async Task<Result> DeleteDistrict(int districtId) =>
            await _nationalSocietyStructureService.DeleteDistrict(districtId);

        /// <summary>
        /// Create a village to National Society's structure
        /// </summary>
        [Route("district/{districtId:int}/village/create"), HttpPost]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.DistrictAccess)]
        public async Task<Result<StructureResponseDto.StructureVillageDto>> CreateVillage(int districtId, [FromBody]StructureEntryRequestDto dto) =>
            await _nationalSocietyStructureService.CreateVillage(districtId, dto.Name);


        /// <summary>
        /// Updates a village to National Society's structure
        /// </summary>
        [Route("village/{villageId}/edit"), HttpPost]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.VillageAccess)]
        public async Task<Result> EditVillage(int villageId, [FromBody]StructureEntryRequestDto dto) =>
            await _nationalSocietyStructureService.EditVillage(villageId, dto.Name);

        /// <summary>
        /// Deletes a village from the National Society's structure
        /// </summary>
        [Route("village/{villageId}/delete"), HttpPost]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.VillageAccess)]
        public async Task<Result> DeleteVillage(int villageId) =>
            await _nationalSocietyStructureService.DeleteVillage(villageId);


        /// <summary>
        /// Create a zone to National Society's structure
        /// </summary>
        [Route("zone/create"), HttpPost]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.VillageAccess)]
        public async Task<Result<StructureResponseDto.StructureZoneDto>> CreateZone(int villageId, [FromBody]StructureEntryRequestDto dto) =>
            await _nationalSocietyStructureService.CreateZone(villageId, dto.Name);


        /// <summary>
        /// Updates a zone to National Society's structure
        /// </summary>
        [Route("zone/{zoneId}/edit"), HttpPost]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ZoneAccess)]
        public async Task<Result> EditZone(int zoneId, [FromBody]StructureEntryRequestDto dto) =>
            await _nationalSocietyStructureService.EditZone(zoneId, dto.Name);

        /// <summary>
        /// Deletes a zone from the National Society's structure
        /// </summary>
        [Route("zone/{zoneId}/delete"), HttpPost]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ZoneAccess)]
        public async Task<Result> DeleteZone(int zoneId) =>
            await _nationalSocietyStructureService.DeleteZone(zoneId);


        /// <summary>
        /// Gets all regions in a National Society
        /// </summary>
        [Route("region/list"), HttpGet]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result<List<RegionResponseDto>>> ListRegions(int nationalSocietyId) =>
            await _nationalSocietyStructureService.ListRegions(nationalSocietyId);

        /// <summary>
        /// Gets all districts in a region
        /// </summary>
        [Route("district/list"), HttpGet]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.RegionAccess)]
        public async Task<Result<List<DistrictResponseDto>>> ListDistricts(int regionId) =>
            await _nationalSocietyStructureService.ListDistricts(regionId);

        /// <summary>
        /// Gets all villages in a district
        /// </summary>
        [Route("village/list"), HttpGet]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.DistrictAccess)]
        public async Task<Result<List<VillageResponseDto>>> ListVillages(int districtId) =>
            await _nationalSocietyStructureService.ListVillages(districtId);

        /// <summary>
        /// Gets all zones in a village
        /// </summary>
        [Route("zone/list"), HttpGet]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.VillageAccess)]
        public async Task<Result<List<ZoneResponseDto>>> ListZones(int villageId) =>
            await _nationalSocietyStructureService.ListZones(villageId);
    }
}
