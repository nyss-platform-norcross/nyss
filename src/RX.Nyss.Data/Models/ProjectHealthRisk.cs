using System.Collections.Generic;

namespace RX.Nyss.Data.Models
{
    public class ProjectHealthRisk
    {
        public int Id { get; set; }

        public string FeedbackMessage { get; set; }

        public string CaseDefinition { get; set; }

        public virtual Project Project { get; set; }

        public int HealthRiskId { get; set; }

        public virtual HealthRisk HealthRisk { get; set; }

        public virtual AlertRule AlertRule { get; set; }

        public virtual ICollection<Report> Reports { get; set; }

        public virtual ICollection<Alert> Alerts { get; set; }

        public virtual ICollection<ProjectHealthRiskAlertRecipient> ProjectHealthRiskAlertRecipients { get; set; }
    }
}
