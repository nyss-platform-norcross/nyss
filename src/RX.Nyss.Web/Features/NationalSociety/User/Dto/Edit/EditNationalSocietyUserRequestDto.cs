namespace RX.Nyss.Web.Features.NationalSociety.User.Dto.Edit
{
    public abstract class EditNationalSocietyUserRequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; }
    }
}
