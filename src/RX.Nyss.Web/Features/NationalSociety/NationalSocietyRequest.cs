using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.NationalSociety
{
    public class NationalSocietyRequest
    {
        public string Name { get; set; }
        public int CountryId { get; set; }
        public int ContentLanguageId { get; set; }
    }
}
