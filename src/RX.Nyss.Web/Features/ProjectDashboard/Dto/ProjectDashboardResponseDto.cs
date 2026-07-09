using System.Collections.Generic;
using RX.Nyss.Web.Services.ReportsDashboard.Dto;

namespace RX.Nyss.Web.Features.ProjectDashboard.Dto
{
    public class ProjectDashboardResponseDto
    {
        public ProjectSummaryResponseDto Summary { get; set; } = new ProjectSummaryResponseDto();

        public ReportByHealthRiskAndDateResponseDto ReportsGroupedByHealthRiskAndDate { get; set; } = new ReportByHealthRiskAndDateResponseDto();

        public IEnumerable<ReportByFeaturesAndDateResponseDto> ReportsGroupedByFeaturesAndDate { get; set; } = new List<ReportByFeaturesAndDateResponseDto>();

        public ReportByFeaturesAndDateResponseDto ReportsGroupedByFeatures { get; set; } = new ReportByFeaturesAndDateResponseDto();

        public IEnumerable<ReportsSummaryMapResponseDto> ReportsGroupedByLocation { get; set; } = new List<ReportsSummaryMapResponseDto>();

        public IEnumerable<DataCollectionPointsReportsByDateDto> DataCollectionPointReportsGroupedByDate { get; set; } = new List<DataCollectionPointsReportsByDateDto>();

        public ReportByVillageAndDateResponseDto ReportsGroupedByVillageAndDate { get; set; } = new ReportByVillageAndDateResponseDto();

        public ReportHistogramResponseDto KeptReportsInEscalatedAlertsHistogramData { get; set; } =
            new ReportHistogramResponseDto();
    }
}
