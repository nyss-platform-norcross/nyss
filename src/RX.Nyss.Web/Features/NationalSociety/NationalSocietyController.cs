using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.NationalSociety.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.NationalSociety
{
    [Route("api/nationalSociety")]
    public class NationalSocietyController : BaseController
    {
        private readonly INationalSocietyService _nationalSocietyService;

        public NationalSocietyController(INationalSocietyService nationalSocietyService)
        {
            _nationalSocietyService = nationalSocietyService;
        }

        /// <summary>
        /// Gets all countries with country codes.
        /// </summary>
        /// <returns></returns>
        [Route("{id}/get"), HttpGet, NeedsRole(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result<NationalSocietyResponseDto>> Get(int id) =>
            await _nationalSocietyService.GetNationalSociety(id);

        /// <summary>
        /// Gets all countries with country codes.
        /// </summary>
        /// <returns></returns>
        [Route("list"), HttpGet, NeedsRole(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result<List<NationalSocietyListResponseDto>>> List() =>
            await _nationalSocietyService.GetNationalSocieties();

        /// <summary>
        /// Creates a new National Society
        /// </summary>
        /// <param name="nationalSociety"></param>
        /// <returns></returns>
        [Route("create"), HttpPost, NeedsRole(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result<int>> Create([FromBody]CreateNationalSocietyRequestDto nationalSociety) => 
            await _nationalSocietyService.CreateNationalSociety(nationalSociety);

        /// <summary>
        /// Edits an existing National Society
        /// </summary>
        /// <param name="nationalSociety"></param>
        /// <returns></returns>
        [Route("{id}/edit"), HttpPost, NeedsRole(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result> Edit(int id, [FromBody]EditNationalSocietyRequestDto nationalSociety) => 
            await _nationalSocietyService.EditNationalSociety(nationalSociety);

        /// <summary>
        /// Removes an existing National Society
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id}/remove"), HttpPost, NeedsRole(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result> Remove(int id) =>
            await _nationalSocietyService.RemoveNationalSociety(id);

        /// <summary>
        /// Gets a list of SMS Gateways assigned to a specified National Society.
        /// </summary>
        /// <returns>A list of SMS Gateways assigned to the National Society</returns>
        [HttpGet("{nationalSocietyId:int}/getSmsGateways")]
        [NeedsRole(Role.Administrator, Role.DataManager, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<List<GatewaySettingResponseDto>>> GetSmsGateways(int nationalSocietyId) =>
            _nationalSocietyService.GetSmsGateways(nationalSocietyId);

        /// <summary>
        /// Adds a new SMS Gateway for a specified National Society.
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a National Society</param>
        /// <param name="gatewaySettingRequestDto">A SMS Gateway settings</param>
        /// <returns>An identifier of a created SMS Gateway setting</returns>
        [HttpPost("{nationalSocietyId:int}/addSmsGateway")]
        [NeedsRole(Role.Administrator, Role.DataManager, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<int>> AddSmsGateway(int nationalSocietyId, [FromBody]GatewaySettingRequestDto gatewaySettingRequestDto) =>
            _nationalSocietyService.AddSmsGateway(nationalSocietyId, gatewaySettingRequestDto);

        /// <summary>
        /// Updates a specified SMS Gateway.
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a National Society</param>
        /// <param name="gatewaySettingId">An identifier of SMS Gateway to be updated</param>
        /// <param name="gatewaySettingRequestDto">A SMS Gateway settings</param>
        /// <returns></returns>
        [HttpPost("{nationalSocietyId:int}/updateSmsGateway/{gatewaySettingId:int}")]
        [NeedsRole(Role.Administrator, Role.DataManager, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result> UpdateSmsGateway(int nationalSocietyId, int gatewaySettingId, [FromBody]GatewaySettingRequestDto gatewaySettingRequestDto) =>
            _nationalSocietyService.UpdateSmsGateway(gatewaySettingId, gatewaySettingRequestDto);

        /// <summary>
        /// Deletes a specified SMS Gateway.
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a National Society</param>
        /// <param name="gatewaySettingId">An identifier of SMS Gateway to be deleted</param>
        /// <returns></returns>
        [HttpPost("{nationalSocietyId:int}/deleteSmsGateway/{gatewaySettingId:int}")]
        [NeedsRole(Role.Administrator, Role.DataManager, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result> UpdateSmsGateway(int nationalSocietyId, int gatewaySettingId) =>
            _nationalSocietyService.DeleteSmsGateway(gatewaySettingId);
    }
}
