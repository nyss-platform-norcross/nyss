using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Managers.Dto
{
    public class GetManagerResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; }
        public Role Role { get; set; }
        public int OrganizationId { get; set; }
        public int? ModemId { get; set; }
    }
}
