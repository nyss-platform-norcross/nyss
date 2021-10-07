namespace RX.Nyss.Data.Models
{
    public static class ReportExtensions
    {
        public static bool IsActivityReport(this Report report) =>
            report.ProjectHealthRisk.HealthRisk.HealthRiskCode == 98
            || report.ProjectHealthRisk.HealthRisk.HealthRiskCode == 99;
    }
}
