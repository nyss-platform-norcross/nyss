using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.TechnicalAdvisors.Dto
{
    public class GetTechnicalAdvisorResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? AdditionalPhoneNumber { get; set; }
        public string Organization { get; set; } = string.Empty;
        public Role Role { get; set; }
        public int OrganizationId { get; set; }
        public int? ModemId { get; set; }
    }
}
