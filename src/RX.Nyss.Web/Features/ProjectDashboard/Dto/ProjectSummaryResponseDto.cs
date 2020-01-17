namespace RX.Nyss.Web.Features.ProjectDashboard.Dto
{
    public class ProjectSummaryResponseDto
    {
        public int ActiveDataCollectorCount { get; set; }

        public int InactiveDataCollectorCount { get; set; }

        public int ReportCount { get; set; }

        public AlertsSummaryResponseDto AlertsSummary { get; set; }

        public DataCollectionPointsSummaryResponse DataCollectionPointSummary { get; set; }

        public int ErrorReportCount { get; set; }
    }

    public class AlertsSummaryResponseDto
    {
        public int Escalated { get; set; }
        public int Dismissed { get; set; }
        public int Closed { get; set; }
    }
}
