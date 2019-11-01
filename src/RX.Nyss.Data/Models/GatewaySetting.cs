using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class GatewaySetting
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ApiKey { get; set; }

        public GatewayType GatewayType { get; set; }

        public int NationalSocietyId { get; set; }
        public virtual NationalSociety NationalSociety { get; set; }
    }
}
