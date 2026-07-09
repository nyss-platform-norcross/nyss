using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Projects.Dto
{
    public class ProjectFormDataResponseDto
    {
        public IEnumerable<ProjectHealthRiskResponseDto> HealthRisks { get; set; } = new List<ProjectHealthRiskResponseDto>();
        public IEnumerable<ProjectFormOrganization> Organizations { get; set; } = new List<ProjectFormOrganization>();
        public IEnumerable<AlertNotHandledRecipientDto> AlertNotHandledRecipients { get; set; } = new List<AlertNotHandledRecipientDto>();

        public class ProjectFormOrganization
        {
            public int Id { get; set; }

            public string Name { get; set; } = string.Empty;
        }

        public class AlertNotHandledRecipientDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;

            public int? OrganizationId { get; set; }
        }
    }
}
