using System;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Alerts.Dto
{
    public class AlertListExportResponseDto
    {
        public int Id { get; set; }

        public DateTime LastReportTimestamp { get; set; }

        public DateTime TriggeredAt { get; set; }

        public DateTime? EscalatedAt { get; set; }

        public DateTime? DismissedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public string HealthRisk { get; set; }

        public int ReportCount { get; set; }

        public string Status { get; set; }

        public EscalatedAlertOutcomes? EscalatedOutcome { get; set; }

        public string Comments { get; set; }

        public string LastReportVillage { get; set; }

        public string LastReportDistrict { get; set; }

        public string LastReportRegion { get; set; }

        public string LastReportZone { get; set; }
        public string Investigation { get; set; }
        public string Outcome { get; set; }
    }
}
