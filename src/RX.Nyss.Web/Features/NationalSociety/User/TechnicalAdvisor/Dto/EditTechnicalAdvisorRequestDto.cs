namespace RX.Nyss.Web.Features.NationalSociety.User.TechnicalAdvisor.Dto
{
    public class EditTechnicalAdvisorRequestDto : IEditNationalSocietyUserRequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; }
    }
}
