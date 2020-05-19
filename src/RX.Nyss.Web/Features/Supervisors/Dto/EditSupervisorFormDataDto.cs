using System.Collections.Generic;
using RX.Nyss.Web.Features.ProjectAlertRecipients.Dto;

namespace RX.Nyss.Web.Features.Supervisors.Dto
{
    public class EditSupervisorFormDataDto
    {
        public List<ListProjectsResponseDto> AvailableProjects { get; set; }

        public class ListProjectsResponseDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsClosed { get; set; }
            public List<ProjectAlertRecipientListResponseDto> AlertRecipients { get; set; }
        }
    }
}
