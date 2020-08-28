using System.Collections.Generic;

namespace RX.Nyss.Web.Features.ProjectAlertRecipients.Dto
{
    public class ProjectAlertRecipientResponseDto
    {
        public int Id { get; set; }

        public int OrganizationId { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        public string Organization { get; set; }

        public IEnumerable<ProjectAlertHealthRiskDto> HealthRisks { get; set; }

        public IEnumerable<ProjectAlertSupervisorsDto> Supervisors { get; set; }
    }
}
