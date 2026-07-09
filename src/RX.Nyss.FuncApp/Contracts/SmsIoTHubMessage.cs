using Newtonsoft.Json;

namespace RX.Nyss.FuncApp.Contracts
{
    public class SmsIoTHubMessage
    {
        [JsonProperty("to")]
        public string To { get; set; } = string.Empty;

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("modem_no", NullValueHandling = NullValueHandling.Ignore)]
        public int? ModemNumber { get; set; }

        [JsonProperty("unicode")]
        public string Unicode { get; set; } = string.Empty;
    }
}
