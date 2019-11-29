namespace RX.Nyss.Web.Features.DataCollector.Dto
{
    public class MapOverviewDataCollectorResponseDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public MapOverviewDataCollectorStatus Status { get; set; }
    }

    public enum MapOverviewDataCollectorStatus
    {
        ReportingCorrectly,
        ReportingWithErrors,
        NotReporting
    }
}
