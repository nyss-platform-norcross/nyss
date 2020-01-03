using System.Collections.Generic;
using RX.Nyss.Web.Features.ProjectDashboard.Dto;

namespace RX.Nyss.Web.Features.Project.Dto
{
    public class ProjectDashboardResponseDto
    {
        public ProjectSummaryResponseDto Summary { get; set; }

        public IEnumerable<ReportByDateResponseDto> ReportsGroupedByDate { get; set; }

        public IEnumerable<ReportByFeaturesAndDateResponseDto> ReportsGroupedByFeaturesAndDate { get; set; }

        public ReportByFeaturesAndDateResponseDto ReportsGroupedByFeatures { get; set; }

        public IEnumerable<ProjectSummaryMapResponseDto> ReportsGroupedByLocation { get; set; }

        public IEnumerable<DataCollectionPointsReportsByDateDto> DataCollectionPointReportsGroupedByDate { get; set; }
    }
}
