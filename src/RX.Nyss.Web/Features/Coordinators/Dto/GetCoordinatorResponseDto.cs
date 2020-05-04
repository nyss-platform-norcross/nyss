using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Coordinators.Dto
{
    public class GetCoordinatorResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; }
        public Role Role { get; set; }
    }
}
