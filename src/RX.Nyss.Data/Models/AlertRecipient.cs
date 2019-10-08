namespace RX.Nyss.Data.Models
{
    public class AlertRecipient
    {
        public int Id { get; set; }

        public AlertRule AlertRule { get; set; }

        public string EmailAddress { get; set; }
    }
}
