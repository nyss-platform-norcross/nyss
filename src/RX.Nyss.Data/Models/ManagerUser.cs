namespace RX.Nyss.Data.Models
{
    public class ManagerUser : User
    {
        public int ModemId { get; set; }
        public virtual GatewayModem Modem { get; set; }
    }
}
