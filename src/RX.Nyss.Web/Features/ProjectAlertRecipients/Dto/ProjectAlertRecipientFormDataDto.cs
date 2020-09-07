using System.Collections.Generic;

namespace RX.Nyss.Web.Features.ProjectAlertRecipients.Dto
{
    public class ProjectAlertRecipientFormDataDto
    {
        public IEnumerable<ProjectAlertOrganization> ProjectOrganizations { get; set; }
        public IEnumerable<ProjectAlertHealthRiskDto> HealthRisks { get; set; }
        public IEnumerable<ProjectAlertSupervisorsDto> Supervisors { get; set; }
    }

    public class ProjectAlertOrganization
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
