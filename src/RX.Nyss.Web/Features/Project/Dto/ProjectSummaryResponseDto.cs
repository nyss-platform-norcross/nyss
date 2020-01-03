namespace RX.Nyss.Web.Features.Project.Dto
{
    public class ProjectSummaryResponseDto
    {
        public int ActiveDataCollectorCount { get; set; }

        public int InactiveDataCollectorCount { get; set; }

        public int ReportCount { get; set; }

        public DataCollectionPointsSummaryResponse DataCollectionPointSummary { get; set; }
    }

    public class DataCollectionPointsSummaryResponse
    {
        public int ReferredToHospitalCount { get; set; }
        public int FromOtherVillagesCount { get; set; }
        public int DeathCount { get; set; }
    }
}
