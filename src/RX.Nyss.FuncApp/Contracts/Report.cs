using RX.Nyss.Common.Utils.DataContract;

namespace RX.Nyss.FuncApp.Contracts
{
    public class Report
    {
        public string Content { get; set; }

        public ReportSource ReportSource { get; set; }

        public override string ToString() => $"{nameof(Content)}: {Content}, {nameof(ReportSource)}: {ReportSource}";
    }
}
