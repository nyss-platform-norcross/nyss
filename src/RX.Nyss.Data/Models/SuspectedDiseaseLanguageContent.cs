namespace RX.Nyss.Data.Models
{
    public class SuspectedDiseaseLanguageContent
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual SuspectedDisease SuspectedDisease { get; set; }

        public int ContentLanguageId { get; set; }

        public virtual ContentLanguage ContentLanguage { get; set; }
    }
}
