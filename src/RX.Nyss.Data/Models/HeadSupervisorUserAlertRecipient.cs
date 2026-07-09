namespace RX.Nyss.Data.Models
{
    public class HeadSupervisorUserAlertRecipient
    {
        public int AlertNotificationRecipientId { get; set; }
        public virtual AlertNotificationRecipient AlertNotificationRecipient { get; set; }
        public int HeadSupervisorId { get; set; }
        public virtual HeadSupervisorUser HeadSupervisor { get; set; }
    }
}
