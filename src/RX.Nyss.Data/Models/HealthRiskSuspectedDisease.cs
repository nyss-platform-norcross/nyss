
namespace RX.Nyss.Data.Models
{
    public class HealthRiskSuspectedDisease
    {
        public int HealthRiskId { get; set; }

        public virtual HealthRisk HealthRisk { get; set; }

        public int SuspectedDiseaseId { get; set; }

        public virtual SuspectedDisease SuspectedDisease { get; set; }

    }
}
