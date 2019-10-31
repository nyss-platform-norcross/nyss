namespace RX.Nyss.Web.Features.NationalSociety.User
{
    public interface IEditNationalSocietyUserRequestDto
    {
        string Name { get; set; }
        string PhoneNumber { get; set; }
        string AdditionalPhoneNumber { get; set; }
        string Organization { get; set; }
    }
}
