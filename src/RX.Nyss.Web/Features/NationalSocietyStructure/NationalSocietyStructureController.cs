using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.NationalSocietyStructure
{
    [Route("api")]
    public class NationalSocietyStructureController : BaseController
    {
        private readonly INationalSocietyStructureService _nationalSocietyStructureService;

        public NationalSocietyStructureController(INationalSocietyStructureService nationalSocietyStructureService)
        {
            _nationalSocietyStructureService = nationalSocietyStructureService;
        }

        /// <summary>
        /// Gets all regions in a National Society
        /// </summary>
        [Route("nationalSociety/{nationalSocietyId:int}/structure/get"), HttpGet]
        [NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result<StructureResponseDto>> GetStructure(int nationalSocietyId) =>
            await _nationalSocietyStructureService.GetStructure(nationalSocietyId);

        /// <summary>
        /// Create a region to National Society's structure
        /// </summary>
        [Route("nationalSociety/{nationalSocietyId:int}/region/create"), HttpPost]
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
        /// Removes a region from the National Society's structure
        /// </summary>
        [Route("region/{regionId}/remove"), HttpPost]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.RegionAccess)]
        public async Task<Result> RemoveRegion(int regionId) =>
            await _nationalSocietyStructureService.RemoveRegion(regionId);

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
        /// Removes a district from the National Society's structure
        /// </summary>
        [Route("district/{districtId}/remove"), HttpPost]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.DistrictAccess)]
        public async Task<Result> RemoveDistrict(int districtId) =>
            await _nationalSocietyStructureService.RemoveDistrict(districtId);

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
        /// Removes a village from the National Society's structure
        /// </summary>
        [Route("village/{villageId}/remove"), HttpPost]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.VillageAccess)]
        public async Task<Result> RemoveVillage(int villageId) =>
            await _nationalSocietyStructureService.RemoveVillage(villageId);


        /// <summary>
        /// Create a zone to National Society's structure
        /// </summary>
        [Route("village/{villageId:int}/zone/create"), HttpPost]
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
        /// Removes a zone from the National Society's structure
        /// </summary>
        [Route("zone/{zoneId}/remove"), HttpPost]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor), NeedsPolicy(Policy.ZoneAccess)]
        public async Task<Result> RemoveZone(int zoneId) =>
            await _nationalSocietyStructureService.RemoveZone(zoneId);


        /// <summary>
        /// Gets all regions in a National Society
        /// </summary>
        [Route("nationalSociety/{nationalSocietyId:int}/region/list"), HttpGet]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result<List<RegionResponseDto>>> GetRegions(int nationalSocietyId) =>
            await _nationalSocietyStructureService.GetRegions(nationalSocietyId);

        /// <summary>
        /// Gets all districts in a region
        /// </summary>
        [Route("region/{regionId:int}/district/list"), HttpGet]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.RegionAccess)]
        public async Task<Result<List<DistrictResponseDto>>> GetDistricts(int regionId) =>
            await _nationalSocietyStructureService.GetDistricts(regionId);

        /// <summary>
        /// Gets all villages in a district
        /// </summary>
        [Route("district/{districtId:int}/village/list"), HttpGet]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.DistrictAccess)]
        public async Task<Result<List<VillageResponseDto>>> GetVillages(int districtId) =>
            await _nationalSocietyStructureService.GetVillages(districtId);

        /// <summary>
        /// Gets all zones in a village
        /// </summary>
        [Route("village/{villageId:int}/zone/list"), HttpGet]
        [NeedsRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor), NeedsPolicy(Policy.VillageAccess)]
        public async Task<Result<List<ZoneResponseDto>>> GetZones(int villageId) =>
            await _nationalSocietyStructureService.GetZones(villageId);
    }
}
