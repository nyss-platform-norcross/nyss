namespace RX.Nyss.Web.Features.NationalSociety.User
{
    public interface ICreateNationalSocietyUserRequestDto
    {
        string Email { get; set; }
        string Name { get; set; }
        string PhoneNumber { get; set; }
        string AdditionalPhoneNumber { get; set; }
        string Organization { get; set; }
    }
}
