using RX.Nyss.Common.Utils.DataContract;

namespace RX.Nyss.FuncApp.Contracts;

public class MTNReport
{
    public string SenderAddress { get; set; } = string.Empty;
    public string ReceiverAddress { get; set; } = string.Empty;
    public string SubmittedDate { get; set; }   = string.Empty;
    public string Message { get; set; }  = string.Empty;
    public string Created { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public ReportSource ReportSource { get; set; }

}
