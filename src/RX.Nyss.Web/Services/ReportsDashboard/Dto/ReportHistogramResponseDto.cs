using System.Collections.Generic;

namespace RX.Nyss.Web.Services.ReportsDashboard.Dto;

public class ReportHistogramResponseDto
{
    public string chartStartDateString { get; set; }
    public string chartEndDateString { get; set; }
    public string chartEndDateEpiWeekString { get; set; }
    public string chartStartDateEpiWeekString { get; set; }
    public DatesGroupingType DateGroupingType { get; set; }
    public IEnumerable<HealthRiskReportsGroupedByDateDto> groupedReports { get; set; }
}
