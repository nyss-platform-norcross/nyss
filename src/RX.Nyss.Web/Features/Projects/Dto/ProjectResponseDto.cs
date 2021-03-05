using System.Collections.Generic;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Alerts.Dto;

namespace RX.Nyss.Web.Features.Projects.Dto
{
    public class ProjectResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool AllowMultipleOrganizations { get; set; }

        public ProjectState State { get; set; }

        public IEnumerable<ProjectHealthRiskResponseDto> ProjectHealthRisks { get; set; }

        public ProjectFormDataResponseDto FormData { get; set; }

        public int ContentLanguageId { get; set; }

        public int NationalSocietyId { get; set; }

        public bool HasCoordinator { get; set; }
    }
}
