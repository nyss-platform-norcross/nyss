namespace RX.Nyss.Web.Features.NationalSocietyDashboard.Dto
{
    public class NationalSocietySummaryResponseDto
    {
        public int ActiveDataCollectorCount { get; set; }

        public int ReportCount { get; set; }

        public NationalSocietyDataCollectionPointsSummaryResponse DataCollectionPointSummary { get; set; }
    }
}
