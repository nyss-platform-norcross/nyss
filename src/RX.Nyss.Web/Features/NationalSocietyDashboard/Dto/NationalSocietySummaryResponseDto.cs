using RX.Nyss.Web.Features.ReportsDashboard.Dto;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard.Dto
{
    public class NationalSocietySummaryResponseDto
    {
        public int ActiveDataCollectorCount { get; set; }

        public int ReportCount { get; set; }

        public DataCollectionPointsSummaryResponse DataCollectionPointSummary { get; set; }

        public int ErrorReportCount { get; set; }

        public AlertsSummaryResponseDto AlertsSummary { get; set; }
    }
}
