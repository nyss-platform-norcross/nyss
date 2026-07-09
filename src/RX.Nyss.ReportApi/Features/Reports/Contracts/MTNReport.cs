using RX.Nyss.Common.Utils.DataContract;

namespace RX.Nyss.ReportApi.Features.Reports.Contracts;

public class MTNReport
{
    public string SenderAddress { get; set; }
    public string ReceiverAddress { get; set; }
    public string SubmittedDate { get; set; }
    public string Message { get; set; }
    public string Created { get; set; }
    public string Id { get; set; }
    public ReportSource ReportSource { get; set; }

}
