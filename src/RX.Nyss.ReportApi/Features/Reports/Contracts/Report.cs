using RX.Nyss.Common.Utils.DataContract;

namespace RX.Nyss.ReportApi.Features.Reports.Contracts;

public class Report
{
    public string Content { get; set; }

    public ReportSource ReportSource { get; set; }

    public override string ToString() => $"{nameof(Content)}: {Content}, {nameof(ReportSource)}: {ReportSource}";
}
