using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Supervisor.Dto
{
    public class GetSupervisorResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public Sex Sex { get; set; }
        public int DecadeOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string AdditionalPhoneNumber { get; set; }
        public int? ProjectId { get; set; }
        public Role Role { get; set; }
    }
}
