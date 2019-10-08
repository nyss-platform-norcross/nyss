using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class HealthRisk
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public HealthRiskType HealthRiskType { get; set; }

        public int HealthRiskCode { get; set; }

        public AlertRule AlertRule { get; set; }
    }
}
