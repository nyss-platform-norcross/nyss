using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.SmsGateways.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.SmsGateways;

[Route("api/smsGateway")]
public class SmsGatewayController : BaseController
{
    private readonly ISmsGatewayService _smsGatewayService;
    private readonly IIotHubService _iotHubService;

    public SmsGatewayController(ISmsGatewayService smsGatewayService, IIotHubService iotHubService)
    {
        _smsGatewayService = smsGatewayService;
        _iotHubService = iotHubService;
    }

    /// <summary>
    /// Gets a SMS Gateway.
    /// </summary>
    /// <returns>A SMS Gateway</returns>
    [HttpGet("{smsGatewayId:int}/get")]
    [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Coordinator), NeedsPolicy(Policy.SmsGatewayAccess)]
    public Task<Result<GatewaySettingResponseDto>> Get(int smsGatewayId) =>
        _smsGatewayService.Get(smsGatewayId);

    /// <summary>
    /// Lists SMS Gateways assigned to a specified national society.
    /// </summary>
    /// <returns>A list of SMS Gateways assigned to the national society</returns>
    [HttpGet("list")]
    [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Coordinator), NeedsPolicy(Policy.NationalSocietyAccess)]
    public Task<Result<List<GatewaySettingResponseDto>>> List(int nationalSocietyId) =>
        _smsGatewayService.List(nationalSocietyId);

    /// <summary>
    /// Creates a new SMS Gateway for a specified national society.
    /// </summary>
    /// <param name="nationalSocietyId">An identifier of a national society</param>
    /// <param name="createRequestDto">A SMS Gateway settings</param>
    /// <returns>An identifier of the created SMS Gateway setting</returns>
    [HttpPost("create")]
    [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Coordinator), NeedsPolicy(Policy.NationalSocietyAccess)]
    public Task<Result<int>> Create(int nationalSocietyId, [FromBody] CreateGatewaySettingRequestDto createRequestDto) =>
        _smsGatewayService.Create(nationalSocietyId, createRequestDto);

    /// <summary>
    /// Edits a specified SMS Gateway.
    /// </summary>
    /// <param name="smsGatewayId">An identifier of SMS Gateway to be updated</param>
    /// <param name="editRequestDto">A SMS Gateway settings</param>
    /// <returns></returns>
    [HttpPost("{smsGatewayId:int}/edit")]
    [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Coordinator), NeedsPolicy(Policy.SmsGatewayAccess)]
    public Task<Result> Edit(int smsGatewayId, [FromBody] EditGatewaySettingRequestDto editRequestDto) =>
        _smsGatewayService.Edit(smsGatewayId, editRequestDto);

    /// <summary>
    /// Deletes a specified SMS Gateway.
    /// </summary>
    /// <param name="smsGatewayId">An identifier of SMS Gateway to be deleted</param>
    /// <returns></returns>
    [HttpPost("{smsGatewayId:int}/delete")]
    [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Coordinator), NeedsPolicy(Policy.SmsGatewayAccess)]
    public Task<Result> Delete(int smsGatewayId) =>
        _smsGatewayService.Delete(smsGatewayId);

    /// <summary>
    /// Get the connectionstring that the SMSGateway should use as a device in order to connect to the IotHub.
    /// </summary>
    /// <param name="smsGatewayId">SmsGateway identifier</param>
    /// <returns></returns>
    [HttpGet("{smsGatewayId:int}/getIotHubConnectionstring")]
    [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Coordinator), NeedsPolicy(Policy.SmsGatewayAccess)]
    public Task<Result> GetIotHubConnectionString(int smsGatewayId) =>
        _smsGatewayService.GetIotHubConnectionString(smsGatewayId);

    /// <summary>
    /// Ping the IoT device the SMSGateway is set up to use.
    /// </summary>
    /// <param name="deviceId">IoT Hub device identifier</param>
    /// <returns></returns>
    [HttpPost("iotDevices/{deviceId}/ping")]
    [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Coordinator)]
    public Task<Result> PingGatewayDevice(string deviceId) =>
        _iotHubService.Ping(deviceId);

    /// <summary>
    /// List available IoT Hub devices
    /// </summary>
    /// <returns></returns>
    [HttpGet("iotDevices/list")]
    [NeedsRole(Role.Administrator, Role.TechnicalAdvisor, Role.Manager, Role.Coordinator)]
    public Task<Result<IEnumerable<string>>> ListIotHubDevices() =>
        _smsGatewayService.ListIotHubDevices();
}
