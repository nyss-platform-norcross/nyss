namespace RX.Nyss.Data.Models
{
    public class Localization
    {
        public string Key { get; set; }

        public ApplicationLanguage ApplicationLanguage { get; set; }

        public int ApplicationLanguageId { get; set; }

        public string Value { get; set; }
    }
}
