using System;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Features.Reports.Models;

namespace RX.Nyss.ReportApi.Features.Reports.Handlers
{
    public class ReportData
    {
        public GatewaySetting GatewaySetting { get; set; }
        public DataCollector DataCollector { get; set; }
        public ParsedReport ParsedReport { get; set; }
        public ProjectHealthRisk ProjectHealthRisk { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}
