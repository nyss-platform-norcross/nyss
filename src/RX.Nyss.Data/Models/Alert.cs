using System;
using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class Alert
    {
        public int Id { get; set; }

        public string? Comments { get; set; }

        public AlertStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? EscalatedAt { get; set; }

        public DateTime? DismissedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public DateTime? RecipientsNotifiedAt { get; set; }

        public EscalatedAlertOutcomes? EscalatedOutcome { get; set; }

        public virtual User? ClosedBy { get; set; }

        public virtual User? DismissedBy { get; set; }

        public virtual User? EscalatedBy { get; set; }
        public virtual ProjectHealthRisk ProjectHealthRisk { get; set; }

        public virtual ICollection<AlertReport> AlertReports { get; set; }
    }
}
