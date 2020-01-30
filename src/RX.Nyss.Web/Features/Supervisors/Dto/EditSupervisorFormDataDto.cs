using System.Collections.Generic;

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
        }
    }
}
