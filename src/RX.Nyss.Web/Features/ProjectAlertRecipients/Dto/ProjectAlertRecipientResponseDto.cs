using System.Collections.Generic;

namespace RX.Nyss.Web.Features.ProjectAlertRecipients.Dto
{
    public class ProjectAlertRecipientResponseDto
    {
        public int Id { get; set; }

        public int OrganizationId { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string Organization { get; set; } = string.Empty;
        public int? ModemId { get; set; }

        public IEnumerable<ProjectAlertHealthRiskDto>? HealthRisks { get; set; }

        public IEnumerable<ProjectAlertSupervisorsDto>? Supervisors { get; set; }

        public IEnumerable<ProjectAlertSupervisorsDto>? HeadSupervisors { get; set; }
    }
}
