namespace RX.Nyss.Web.Features.NationalSociety.User.TechnicalAdvisor.Dto
{
    public class CreateTechnicalAdvisorRequestDto: ICreateNationalSocietyUserRequestDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; }
    }
}
