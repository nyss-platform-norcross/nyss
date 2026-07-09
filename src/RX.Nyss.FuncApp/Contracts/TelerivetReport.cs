using RX.Nyss.Common.Utils.DataContract;

namespace RX.Nyss.FuncApp.Contracts
{
    public class TelerivetReport
    {
        public string TimeCreated { get; set; } = string.Empty;
        public string TimeUpdated { get; set; } = string.Empty;
        public string MessageContent { get; set; } = string.Empty;
        public string FromNumber { get; set; }  = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public ReportSource ReportSource { get; set; }

    }
}
