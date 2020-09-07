namespace RX.Nyss.Data.Models
{
    public class ProjectHealthRiskAlertRecipient
    {
        public int AlertNotificationRecipientId { get; set; }
        public virtual AlertNotificationRecipient AlertNotificationRecipient { get; set; }
        public int ProjectHealthRiskId { get; set; }
        public virtual ProjectHealthRisk ProjectHealthRisk { get; set; }
    }
}
