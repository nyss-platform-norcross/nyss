using System.Collections.Generic;
using RX.Nyss.Web.Features.Projects.Dto;

namespace RX.Nyss.Web.Features.ProjectDashboard.Dto
{
    public class ProjectDashboardResponseDto
    {
        public ProjectSummaryResponseDto Summary { get; set; } = new ProjectSummaryResponseDto();

        public IEnumerable<ReportByDateResponseDto> ReportsGroupedByDate { get; set; } = new List<ReportByDateResponseDto>();

        public IEnumerable<ReportByFeaturesAndDateResponseDto> ReportsGroupedByFeaturesAndDate { get; set; } = new List<ReportByFeaturesAndDateResponseDto>();

        public ReportByFeaturesAndDateResponseDto ReportsGroupedByFeatures { get; set; } = new ReportByFeaturesAndDateResponseDto();

        public IEnumerable<ProjectSummaryMapResponseDto> ReportsGroupedByLocation { get; set; } = new List<ProjectSummaryMapResponseDto>();

        public IEnumerable<DataCollectionPointsReportsByDateDto> DataCollectionPointReportsGroupedByDate { get; set; } = new List<DataCollectionPointsReportsByDateDto>();

        public ReportByVillageAndDateResponseDto ReportsGroupedByVillageAndDate { get; set; } = new ReportByVillageAndDateResponseDto();
    }
}
