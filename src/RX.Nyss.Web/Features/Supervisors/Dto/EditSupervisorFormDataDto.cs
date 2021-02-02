using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Supervisors.Dto
{
    public class EditSupervisorFormDataDto
    {
        public List<ListProjectsResponseDto> AvailableProjects { get; set; }
        public List<HeadSupervisorResponseDto> HeadSupervisors { get; set; }

        public class ListProjectsResponseDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsClosed { get; set; }
        }

        public class HeadSupervisorResponseDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
