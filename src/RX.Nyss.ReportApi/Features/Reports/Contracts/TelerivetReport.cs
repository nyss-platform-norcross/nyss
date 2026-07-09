using RX.Nyss.Common.Utils.DataContract;

namespace RX.Nyss.ReportApi.Features.Reports.Contracts;

public class TelerivetReport
{
    public string TimeCreated { get; set; }
    public string TimeUpdated { get; set; }
    public string MessageContent { get; set; }
    public string FromNumber { get; set; }
    public string ApiKey { get; set; }
    public string ProjectId { get; set; }
    public ReportSource ReportSource { get; set; }

}
