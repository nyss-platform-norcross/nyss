namespace RX.Nyss.ReportApi.Features.Common.Contracts
{
    public class SupervisorSmsRecipient
    {
        public int UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public int? Modem { get; set; }
    }
}
