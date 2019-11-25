namespace RX.Nyss.Web.Features.ProjectDashboard.Dto
{
    public class ProjectSummaryMapResponseDto
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
