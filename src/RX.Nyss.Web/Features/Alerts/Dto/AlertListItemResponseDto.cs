using System;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Alerts.Dto
{
    public class AlertListItemResponseDto
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public string HealthRisk { get; set; }

        public int ReportCount { get; set; }

        public string LastReportVillage { get; set; }

        public string Status { get; set; }

        public EscalatedAlertOutcomes? EscalatedOutcome { get; set; }

        public string Comments { get; set; }

        public string LastReportDistrict { get; set; }

        public string LastReportRegion { get; set; }
    }
}
