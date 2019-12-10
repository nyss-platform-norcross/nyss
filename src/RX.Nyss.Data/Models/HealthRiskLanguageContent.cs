namespace RX.Nyss.Data.Models
{
    public class HealthRiskLanguageContent
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CaseDefinition { get; set; }

        public string FeedbackMessage { get; set; }

        public virtual HealthRisk HealthRisk { get; set; }

        public int ContentLanguageId { get; set; }
        public virtual ContentLanguage ContentLanguage { get; set; }
    }
}
