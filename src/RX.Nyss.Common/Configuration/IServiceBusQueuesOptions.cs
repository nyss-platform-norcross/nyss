namespace RX.Nyss.Common.Configuration;

public interface IServiceBusQueuesOptions
{
    string SendEmailQueue { get; set; }
    string ReportDismissalQueue { get; set; }
    string ReportResetQueue { get; set; }
    string CheckAlertQueue { get; set; }
    string RecalculateAlertsQueue { get; set; }
    string SendSmsQueue { get; set; }
    string EidsrReportQueue { get; set; }
    string DhisReportQueue { get; set; }
}

public class ServiceBusQueuesOptions : IServiceBusQueuesOptions
{
    public string SendEmailQueue { get; set; } = string.Empty;
    public string ReportDismissalQueue { get; set; } = string.Empty;
    public string ReportResetQueue { get; set; } = string.Empty;
    public string CheckAlertQueue { get; set; } = string.Empty;
    public string RecalculateAlertsQueue { get; set; } = string.Empty;
    public string SendSmsQueue { get; set; } = string.Empty;
    public string EidsrReportQueue { get; set; } = string.Empty; //For Alerts
    public string DhisReportQueue { get; set; } = string.Empty; //For reports
}