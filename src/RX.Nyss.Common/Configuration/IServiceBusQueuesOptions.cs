namespace RX.Nyss.Common.Configuration
{
    public interface IServiceBusQueuesOptions
    {
        string SendEmailQueue { get; set; }
        string ReportDismissalQueue { get; set; }
        string ReportResetQueue { get; set; }
        string CheckAlertQueue { get; set; }
        string SendSmsQueue { get; set; }
    }

    public class ServiceBusQueuesOptions : IServiceBusQueuesOptions
    {
        public string SendEmailQueue { get; set; }
        public string ReportDismissalQueue { get; set; }
        public string ReportResetQueue { get; set; }
        public string CheckAlertQueue { get; set; }
        public string SendSmsQueue { get; set; }
    }
}
