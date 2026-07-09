namespace RX.Nyss.Data.Models
{
    public class GatewayModem
    {
        public int Id { get; set; }
        public int ModemId { get; set; }
        public string? Name { get; set; }
        public int GatewaySettingId { get; set; }
        public virtual GatewaySetting? GatewaySetting { get; set; }
    }
}
