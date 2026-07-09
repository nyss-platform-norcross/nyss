using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Supervisors.Dto
{
    public class EditSupervisorFormDataDto
    {
        public List<ListProjectsResponseDto> AvailableProjects { get; set; } = new List<ListProjectsResponseDto>();
        public List<HeadSupervisorResponseDto> HeadSupervisors { get; set; } = new List<HeadSupervisorResponseDto>();

        public class ListProjectsResponseDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public bool IsClosed { get; set; }
        }

        public class HeadSupervisorResponseDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
