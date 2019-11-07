using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class HealthRisk
    {
        public int Id { get; set; }

        public HealthRiskType HealthRiskType { get; set; }

        public int HealthRiskCode { get; set; }

        public int AlertRuleId { get; set; }    
        public virtual AlertRule AlertRule { get; set; }

        public virtual ICollection<HealthRiskLanguageContent> LanguageContents { get; set; }
    }
}
