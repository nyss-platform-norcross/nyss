using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.NationalSociety
{
    public class NationalSocietyRequest
    {
        public string Name { get; set; }
        public Country Country { get; set; }
        public ContentLanguageRequest ContentLanguage { get; set; }
    }
}
