namespace RX.Nyss.Data.Models
{
    public class SmsAlertRecipient
    {
        public int Id { get; set; }

        public string PhoneNumber { get; set; }
        
        public virtual Project Project { get; set; }
    }
}
