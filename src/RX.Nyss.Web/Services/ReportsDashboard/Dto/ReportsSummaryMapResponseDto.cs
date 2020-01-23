namespace RX.Nyss.Web.Services.ReportsDashboard.Dto
{
    public class ReportsSummaryMapResponseDto
    {
        public MapReportLocation Location { get; set; }

        public int ReportsCount { get; set; }

        public class MapReportLocation
        {
            public double Latitude { get; set; }

            public double Longitude { get; set; }
        }
    }
}
