using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Project.Dto
{
    public class ProjectResponseDto : ProjectRequestDto
    {
        public int Id { get; set; }

        public ProjectState State { get; set; }
    }
}
