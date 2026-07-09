using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.SmsGateways.Dto
{
    public class GatewaySettingResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string? EmailAddress { get; set; }
        public GatewayType GatewayType { get; set; }
        public string? IotHubDeviceName { get; set; }
        public string? ModemOneName { get; set; }
        public string? ModemTwoName { get; set; }
        public string? TelerivetApiKey { get; set; }
        public string? TelerivetProjectId { get; set; }
        public string? GatewayApiKey { get; set; }
        public string? GatewayApiKeyName { get; set; }
        public string? GatewayExtraKey { get; set; }
        public string? GatewayExtraKeyName { get; set; }
        public string? GatewayUrl { get; set; }
        public string? GatewayAuthUrl { get; set; }
        public string? GatewaySenderId { get; set; }
    }
}
