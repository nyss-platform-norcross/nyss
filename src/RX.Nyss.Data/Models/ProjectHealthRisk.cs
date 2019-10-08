using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class ProjectHealthRisk
    {
        public int Id { get; set; }

        public Project Project { get; set; }

        public HealthRisk HealthRisk { get; set; }

        public string FeedbackMessage { get; set; }

        public AlertRule AlertRule { get; set; }
    }
}
