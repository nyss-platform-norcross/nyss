using System.Collections.Generic;

namespace RX.Nyss.Web.Features.HeadSupervisors.Dto
{
    public class EditHeadSupervisorFormDataDto
    {
        public List<ListProjectsResponseDto> AvailableProjects { get; set; } = new List<ListProjectsResponseDto>();

        public class ListProjectsResponseDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public bool IsClosed { get; set; }
        }
    }
}
