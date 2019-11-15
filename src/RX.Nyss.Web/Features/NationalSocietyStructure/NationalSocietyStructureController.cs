using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
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
        [Route("nationalSociety/{nationalSocietyId:int}/region/list"), HttpGet, NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer)]
        public async Task<Result<List<RegionResponseDto>>> GetRegions(int nationalSocietyId) =>
            await _nationalSocietyStructureService.GetRegions(nationalSocietyId);

        /// <summary>
        /// Gets all districts in a region
        /// </summary>
        [Route("region/{regionId:int}/district/list"), HttpGet, NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer)]
        public async Task<Result<List<DistrictResponseDto>>> GetDistricts(int regionId) =>
            await _nationalSocietyStructureService.GetDistricts(regionId);

        /// <summary>
        /// Gets all villages in a district
        /// </summary>
        [Route("district/{districtId:int}/village/list"), HttpGet, NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer)]
        public async Task<Result<List<VillageResponseDto>>> GetVillages(int districtId) =>
            await _nationalSocietyStructureService.GetVillages(districtId);

        /// <summary>
        /// Gets all zones in a village
        /// </summary>
        [Route("village/{villageId:int}/zone/list"), HttpGet, NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer)]
        public async Task<Result<List<ZoneResponseDto>>> GetZones(int villageId) =>
            await _nationalSocietyStructureService.GetZones(villageId);
    }
}
