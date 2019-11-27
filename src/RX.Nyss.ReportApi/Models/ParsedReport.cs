using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.ReportApi.Models
{
    public class ParsedReport
    {
        public int HealthRiskCode { get; set; }

        public ReportType ReportType { get; set; }

        public ReportCase ReportedCase { get; set; } = new ReportCase();

        public DataCollectorType DataCollectorType { get; set; }

        public DataCollectionPointCase DataCollectionPointCase { get; set; } = new DataCollectionPointCase();
    }
}
