namespace RX.Nyss.Web.Features.NationalSociety
{
    public class EditNationalSocietyRequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CountryId { get; set; }
        public int ContentLanguageId { get; set; }
    }
}
