namespace RX.Nyss.Data.Models
{
    public class SupervisorUserAlertRecipient
    {
        public int AlertNotificationRecipientId { get; set; }
        public virtual AlertNotificationRecipient AlertNotificationRecipient { get; set; }
        public int SupervisorId { get; set; }
        public virtual SupervisorUser Supervisor { get; set; }
    }
}
