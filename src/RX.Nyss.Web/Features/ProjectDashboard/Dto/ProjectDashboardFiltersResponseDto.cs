using System.Collections.Generic;

namespace RX.Nyss.Web.Features.ProjectDashboard.Dto
{
    public class ProjectDashboardFiltersResponseDto
    {
        public IEnumerable<HealthRiskDto> HealthRisks { get; set; }

        public class HealthRiskDto
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
