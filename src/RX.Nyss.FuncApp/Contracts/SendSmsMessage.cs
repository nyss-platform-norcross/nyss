namespace RX.Nyss.FuncApp.Contracts
{
    public class SendSmsMessage
    {
        public string IotHubDeviceName { get; set; }

        public string PhoneNumber { get; set; }

        public string SmsMessage { get; set; }
        public int? ModemNumber { get; set; }
    }
}
