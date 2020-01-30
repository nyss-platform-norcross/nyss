using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.SmsGateways.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.SmsGateways
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
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.SmsGatewayAccess)]
        public Task<Result<GatewaySettingResponseDto>> Get(int smsGatewayId) =>
            _smsGatewayService.Get(smsGatewayId);

        /// <summary>
        /// Lists SMS Gateways assigned to a specified national society.
        /// </summary>
        /// <returns>A list of SMS Gateways assigned to the national society</returns>
        [HttpGet("list")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<List<GatewaySettingResponseDto>>> List(int nationalSocietyId) =>
            _smsGatewayService.List(nationalSocietyId);

        /// <summary>
        /// Creates a new SMS Gateway for a specified national society.
        /// </summary>
        /// <param name="nationalSocietyId">An identifier of a national society</param>
        /// <param name="gatewaySettingRequestDto">A SMS Gateway settings</param>
        /// <returns>An identifier of the created SMS Gateway setting</returns>
        [HttpPost("create")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.NationalSocietyAccess)]
        public Task<Result<int>> Create(int nationalSocietyId, [FromBody] GatewaySettingRequestDto gatewaySettingRequestDto) =>
            _smsGatewayService.Create(nationalSocietyId, gatewaySettingRequestDto);

        /// <summary>
        /// Edits a specified SMS Gateway.
        /// </summary>
        /// <param name="smsGatewayId">An identifier of SMS Gateway to be updated</param>
        /// <param name="gatewaySettingRequestDto">A SMS Gateway settings</param>
        /// <returns></returns>
        [HttpPost("{smsGatewayId:int}/edit")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.SmsGatewayAccess)]
        public Task<Result> Edit(int smsGatewayId, [FromBody] GatewaySettingRequestDto gatewaySettingRequestDto) =>
            _smsGatewayService.Edit(smsGatewayId, gatewaySettingRequestDto);

        /// <summary>
        /// Deletes a specified SMS Gateway.
        /// </summary>
        /// <param name="smsGatewayId">An identifier of SMS Gateway to be deleted</param>
        /// <returns></returns>
        [HttpPost("{smsGatewayId:int}/delete")]
        [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager), NeedsPolicy(Policy.SmsGatewayAccess)]
        public Task<Result> Delete(int smsGatewayId) =>
            _smsGatewayService.Delete(smsGatewayId);
    }
}
