using System.Collections.Generic;
using RX.Nyss.Data.Models;

namespace RX.Nyss.TestData.TestDataGeneration
{
    public class EntityData
    {
        public List<Report> Reports { get; set; } = new List<Report>();
        public List<RX.Nyss.Data.Models.Alert> Alerts { get; set; } = new List<RX.Nyss.Data.Models.Alert>();
        public List<AlertReport> AlertReports { get; set; } = new List<AlertReport>();
        public List<AlertRule> AlertRules { get; set; } = new List<AlertRule>();
        public List<HealthRisk> HealthRisks { get; set; } = new List<HealthRisk>();
        public List<ProjectHealthRisk> ProjectHealthRisks { get; set; } = new List<ProjectHealthRisk>();
    }
}
