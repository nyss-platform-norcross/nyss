namespace RX.Nyss.Web.Features.NationalSociety.User.DataManager.Dto
{
    public class EditDataManagerRequestDto:IEditNationalSocietyUserRequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; }
    }
}
