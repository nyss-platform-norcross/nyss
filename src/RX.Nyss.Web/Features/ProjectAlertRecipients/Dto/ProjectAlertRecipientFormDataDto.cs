using System.Collections.Generic;

namespace RX.Nyss.Web.Features.ProjectAlertRecipients.Dto
{
    public class ProjectAlertRecipientFormDataDto
    {
        public IEnumerable<ProjectAlertOrganization> ProjectOrganizations { get; set; }
        public IEnumerable<ProjectAlertHealthRisk> HealthRisks { get; set; }
        public IEnumerable<ProjectAlertSupervisors> Supervisors { get; set; }
    }

    public class ProjectAlertOrganization
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ProjectAlertSupervisors
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OrganizationId { get; set; }
    }

    public class ProjectAlertHealthRisk
    {
        public int Id { get; set; }
        public string HealthRiskName { get; set; }
    }
}
