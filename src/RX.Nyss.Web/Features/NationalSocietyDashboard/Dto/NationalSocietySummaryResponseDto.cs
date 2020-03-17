using RX.Nyss.Web.Services.ReportsDashboard.Dto;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard.Dto
{
    public class NationalSocietySummaryResponseDto
    {
        public int ActiveDataCollectorCount { get; set; }
        public int InactiveDataCollectorCount { get; set; }

        public int ReportCount { get; set; }

        public DataCollectionPointsSummaryResponse DataCollectionPointSummary { get; set; } = new DataCollectionPointsSummaryResponse();

        public int ErrorReportCount { get; set; }

        public AlertsSummaryResponseDto AlertsSummary { get; set; } = new AlertsSummaryResponseDto();
        public int NumberOfVillages { get; set; }
        public int NumberOfDistricts { get; set; }
    }
}
