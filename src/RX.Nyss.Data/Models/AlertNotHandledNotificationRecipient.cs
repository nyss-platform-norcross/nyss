namespace RX.Nyss.Data.Models
{
    public class AlertNotHandledNotificationRecipient
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }

        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }
    }
}
