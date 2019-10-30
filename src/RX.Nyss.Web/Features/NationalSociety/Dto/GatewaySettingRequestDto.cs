using System;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.NationalSociety.Dto
{
    public class GatewaySettingRequestDto
    {
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public GatewayType GatewayType { get; set; }
    }
}
