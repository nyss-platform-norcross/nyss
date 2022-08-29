namespace RX.Nyss.Data.Models
{
    public class ApplicationLanguage
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string LanguageCode { get; set; }

        public void SetDisplayLanguageId(int displayLanguageId)
        {
            Id = displayLanguageId;
        }

        public void SetDisplayLanguageName(string displayLanguageName)
        {
            DisplayName = displayLanguageName;
        }

        public void SetDisplayLanguageCode(string displayLanguageCode)
        {
            LanguageCode = displayLanguageCode;
        }
    }
}
