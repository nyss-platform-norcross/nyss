namespace RX.Nyss.Web.Features.NationalSocietyDashboard.Dto
{
    public class NationalSocietySummaryMapResponseDto
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
