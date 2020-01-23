namespace RX.Nyss.Web.Services.ReportsDashboard.Dto
{
    public class DataCollectionPointsReportsByDateDto
    {
        public string Period { get; set; }

        public int ReferredCount { get; set; }

        public int DeathCount { get; set; }

        public int FromOtherVillagesCount { get; set; }
    }
}
