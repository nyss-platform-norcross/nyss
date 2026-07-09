using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Projects.Dto
{
    public class ProjectResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool AllowMultipleOrganizations { get; set; }

        public ProjectState State { get; set; }

        public IEnumerable<ProjectHealthRiskResponseDto> ProjectHealthRisks { get; set; } = new List<ProjectHealthRiskResponseDto>();

        public ProjectFormDataResponseDto FormData { get; set; } = new ProjectFormDataResponseDto();

        public int ContentLanguageId { get; set; }

        public int NationalSocietyId { get; set; }

        public bool HasCoordinator { get; set; }
    }
}
