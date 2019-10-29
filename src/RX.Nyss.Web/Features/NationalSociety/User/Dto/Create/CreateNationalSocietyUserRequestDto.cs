namespace RX.Nyss.Web.Features.NationalSociety.User.Dto.Create
{
    public abstract class CreateNationalSocietyUserRequestDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; }
        public int NationalSocietyId { get; set; }
    }
}
