using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.HealthRisk
{
    public class HealthRiskResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public HealthRiskType HealthRiskType { get; set; }

        public int HealthRiskCode { get; set; }
    }
}
