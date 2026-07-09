using System;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Alerts.Dto
{
    public class AlertListItemResponseDto
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public string HealthRisk { get; set; } = string.Empty;

        public int ReportCount { get; set; }

        public string LastReportVillage { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public EscalatedAlertOutcomes? EscalatedOutcome { get; set; }

        public string? Comments { get; set; }

        public string LastReportDistrict { get; set; } = string.Empty;

        public string LastReportRegion { get; set; } = string.Empty;

        public string? LastReportZone { get; set; }
    }
}
