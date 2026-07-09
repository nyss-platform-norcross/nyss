namespace RX.Nyss.FuncApp.Contracts
{
    public class SendSmsMessage
    {
        public string IotHubDeviceName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string SmsMessage { get; set; } = string.Empty;

        public int? ModemNumber { get; set; }
    }
}
