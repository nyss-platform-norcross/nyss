namespace RX.Nyss.Web.Features.NationalSociety
{
    public class CreateNationalSocietyRequestDto
    {
        public string Name { get; set; }
        public int CountryId { get; set; }
        public int ContentLanguageId { get; set; }
    }
}
