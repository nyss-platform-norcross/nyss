namespace RX.Nyss.Data.Models
{
    public class TechnicalAdvisorUserGatewayModem
    {
        public int TechnicalAdvisorUserId { get; set; }
        public virtual TechnicalAdvisorUser? TechnicalAdvisorUser { get; set; }
        public int GatewayModemId { get; set; }
        public virtual GatewayModem? GatewayModem { get; set; }
    }
}
