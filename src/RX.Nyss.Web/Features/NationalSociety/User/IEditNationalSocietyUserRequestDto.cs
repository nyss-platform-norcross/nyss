namespace RX.Nyss.Web.Features.NationalSociety.User
{
    public interface IEditNationalSocietyUserRequestDto
    {
        int Id { get; set; }
        string Name { get; set; }
        string PhoneNumber { get; set; }
        string AdditionalPhoneNumber { get; set; }
        string Organization { get; set; }
    }
}
