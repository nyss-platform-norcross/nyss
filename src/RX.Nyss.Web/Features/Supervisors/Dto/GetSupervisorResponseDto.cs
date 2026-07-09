using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Supervisors.Dto
{
    public class GetSupervisorResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Sex Sex { get; set; }
        public int DecadeOfBirth { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? AdditionalPhoneNumber { get; set; }
        public int? ProjectId { get; set; }
        public Role Role { get; set; }
        public string Organization { get; set; } = string.Empty;
        public int? OrganizationId { get; set; }
        public int NationalSocietyId { get; set; }
        public int? HeadSupervisorId { get; set; }
        public int? ModemId { get; set; }
        public EditSupervisorFormDataDto.ListProjectsResponseDto CurrentProject { get; set; } = new EditSupervisorFormDataDto.ListProjectsResponseDto();
        public EditSupervisorFormDataDto EditSupervisorFormData { get; set; } = new EditSupervisorFormDataDto();
    }
}
