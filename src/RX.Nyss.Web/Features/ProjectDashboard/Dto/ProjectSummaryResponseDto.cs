using RX.Nyss.Web.Services.ReportsDashboard.Dto;

namespace RX.Nyss.Web.Features.ProjectDashboard.Dto
{
    public class ProjectSummaryResponseDto
    {
        public int KeptReportCount { get; set; }
        public int DismissedReportCount { get; set; }
        public int NotCrossCheckedReportCount { get; set; }
        public int TotalReportCount { get; set; }
        public int ActiveDataCollectorCount { get; set; }
        public int InactiveDataCollectorCount { get; set; }
        public DataCollectionPointsSummaryResponse DataCollectionPointSummary { get; set; } = new DataCollectionPointsSummaryResponse();
        public AlertsSummaryResponseDto AlertsSummary { get; set; } = new AlertsSummaryResponseDto();
        public int NumberOfVillages { get; set; }
        public int NumberOfDistricts { get; set; }
    }
}
