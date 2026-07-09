using Newtonsoft.Json;

namespace RX.Nyss.ReportApi.Features.Common.Contracts
{
    public class SmsIoTHubMessage
    {
        [JsonProperty("to")]
        public string To { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("modem_no", NullValueHandling = NullValueHandling.Ignore)]
        public int? ModemNumber { get; set; }
        [JsonProperty("unicode")]
        public string Unicode { get; set; }
    }
}
