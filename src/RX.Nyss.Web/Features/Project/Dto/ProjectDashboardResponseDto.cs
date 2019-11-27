using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Project.Dto
{
    public class ProjectDashboardResponseDto
    {
        public ProjectSummaryResponseDto Summary { get; set; }

        public IEnumerable<ReportByDateResponseDto> ReportsGroupedByDate { get; set; }
    }
}
