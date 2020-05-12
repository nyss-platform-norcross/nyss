using System.Collections.Generic;

namespace RX.Nyss.Data.Models
{
    public class AlertNotificationRecipient
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string Organization { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int ProjectId { get; set; }
        public virtual ICollection<SupervisorUserAlertRecipient> SupervisorAlertRecipients { get; set; }
    }
}
