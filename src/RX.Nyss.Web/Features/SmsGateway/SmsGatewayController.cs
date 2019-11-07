using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Authentication.Policies;
using RX.Nyss.Web.Features.SmsGateway.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.SmsGateway
{
    [Route("api/smsGateway")]
    public class SmsGatewayController : BaseController
    {
        private readonly ISmsGatewayService _smsGatewayService;

        public SmsGatewayController(ISmsGatewayService smsGatewayService)
        {
            _smsGatewayService = smsGatewayService;
        }

        /// <summary>
        /// Gets a SMS Gateway.
        /// </summary>
        /// <returns>A SMS Gateway</returns>
        [HttpGet("{smsGatewayId:int}/get")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.DataManager, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.SmsGatewayAccess)]
        public Task<Result<GatewaySettingResponseDto>> GetSmsGateway(int smsGatewayId) =>
            _smsGatewayService.GetSmsGateway(smsGatewayId);

        /// <summary>
        /// Lists SMS Gateways assigned to a specified National Society.
        /// </summary>
        /// <returns>A list of SMS Gateways assigned to the National Society</returns>
        [HttpGet("list/nationalSociety/{nationalSocietyId:int}")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.DataManager, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<List<GatewaySettingResponseDto>>> GetSmsGateways(int nationalSocietyId) =>
            _smsGatewayService.GetSmsGateways(nationalSocietyId);

        /// <summary>
        /// Adds a new SMS Gateway for a specified National Society.
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a National Society</param>
        /// <param name="gatewaySettingRequestDto">A SMS Gateway settings</param>
        /// <returns>An identifier of a created SMS Gateway setting</returns>
        [HttpPost("add/nationalSociety/{nationalSocietyId:int}")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.DataManager, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<int>> AddSmsGateway(int nationalSocietyId, [FromBody]GatewaySettingRequestDto gatewaySettingRequestDto) =>
            _smsGatewayService.AddSmsGateway(nationalSocietyId, gatewaySettingRequestDto);

        /// <summary>
        /// Updates a specified SMS Gateway.
        /// </summary>
        /// <param name="smsGatewayId">An identifier of SMS Gateway to be updated</param>
        /// <param name="gatewaySettingRequestDto">A SMS Gateway settings</param>
        /// <returns></returns>
        [HttpPost("{smsGatewayId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.DataManager, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.SmsGatewayAccess)]
        public Task<Result> UpdateSmsGateway(int smsGatewayId, [FromBody]GatewaySettingRequestDto gatewaySettingRequestDto) =>
            _smsGatewayService.UpdateSmsGateway(smsGatewayId, gatewaySettingRequestDto);

        /// <summary>
        /// Removes a specified SMS Gateway.
        /// </summary>
        /// <param name="smsGatewayId">An identifier of SMS Gateway to be removed</param>
        /// <returns></returns>
        [HttpPost("{smsGatewayId:int}/remove")]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.DataManager, Role.TechnicalAdvisor)]
        [NeedsPolicy(Policy.SmsGatewayAccess)]
        public Task<Result> UpdateSmsGateway(int smsGatewayId) =>
            _smsGatewayService.DeleteSmsGateway(smsGatewayId);
    }
}