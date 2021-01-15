using System.Collections.Generic;
using RX.Nyss.Web.Features.ProjectAlertRecipients.Dto;

namespace RX.Nyss.Web.Features.HeadSupervisors.Dto
{
    public class EditHeadSupervisorFormDataDto
    {
        public List<ListProjectsResponseDto> AvailableProjects { get; set; }

        public class ListProjectsResponseDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsClosed { get; set; }
        }
    }
}
