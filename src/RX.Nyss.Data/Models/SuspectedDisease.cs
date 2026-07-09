using System.Collections.Generic;

namespace RX.Nyss.Data.Models
{
    public class SuspectedDisease
    {
        public int Id { get; set; }

        public int SuspectedDiseaseCode { get; set; }

        public virtual ICollection<SuspectedDiseaseLanguageContent>? LanguageContents { get; set; }

        public virtual ICollection<HealthRiskSuspectedDisease>? HealthRiskSuspectedDiseases { get; set; }

    }
}
