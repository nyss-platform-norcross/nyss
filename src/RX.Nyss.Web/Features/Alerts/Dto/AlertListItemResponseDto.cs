using System;

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
    }
}
