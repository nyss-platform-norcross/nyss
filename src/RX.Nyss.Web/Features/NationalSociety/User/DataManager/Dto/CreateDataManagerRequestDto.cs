namespace RX.Nyss.Web.Features.NationalSociety.User.DataManager.Dto
{
    public class CreateDataManagerRequestDto :ICreateNationalSocietyUserRequestDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; }
        public int NationalSocietyId { get; set; }
    }
}
