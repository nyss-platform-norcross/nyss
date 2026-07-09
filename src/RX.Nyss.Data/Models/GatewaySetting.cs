using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class GatewaySetting
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? ApiKey { get; set; }

        public GatewayType GatewayType { get; set; }

        public string? EmailAddress { get; set; }

        public int NationalSocietyId { get; set; }

        public virtual NationalSociety? NationalSociety { get; set; }

        public string? IotHubDeviceName { get; set; }
        public virtual ICollection<GatewayModem>? Modems { get; set; }

        public string? TelerivetSendSmsApiKey { get; set; }

        public string? TelerivetProjectId { get; set; }

        public string? GatewayApiKey { get; set; }

        public string? GatewayApiKeyName { get; set; }

        public string? GatewayExtraKey { get; set; }

        public string? GatewayExtraKeyName { get; set; }

        public string? GatewayUrl { get; set; }

        public string? GatewaySenderId { get; set; }

        public string? GatewayAuthUrl { get; set; }

    }
}
