using RX.Nyss.Web.Services.ReportsDashboard.Dto;

namespace RX.Nyss.Web.Features.ProjectDashboard.Dto
{
    public class ProjectSummaryResponseDto
    {
        public int ActiveDataCollectorCount { get; set; }
        public int InactiveDataCollectorCount { get; set; }
        public int ReportCount { get; set; }
        public AlertsSummaryResponseDto AlertsSummary { get; set; } = new AlertsSummaryResponseDto();
        public DataCollectionPointsSummaryResponse DataCollectionPointSummary { get; set; } = new DataCollectionPointsSummaryResponse();
        public int ErrorReportCount { get; set; }
        public int NumberOfVillages { get; set; }
        public int NumberOfDistricts { get; set; }
        public int DismissedReportCount { get; set; }
        public int KeptReportCount { get; set; }
        public int PendingReportCount { get; set; }
    }
}
