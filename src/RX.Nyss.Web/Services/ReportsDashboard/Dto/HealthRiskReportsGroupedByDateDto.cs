using System.Collections.Generic;
using System.Linq;

namespace RX.Nyss.Web.Services.ReportsDashboard.Dto;

public class HealthRiskReportsGroupedByDateDto
{
    public int HealthRiskId { get; set; }
    public string HealthRiskName { get; set; }

    public IEnumerable<PeriodDto> Data { get; set; }

    public bool ValueEquals(object obj)
    {
        var other = (HealthRiskReportsGroupedByDateDto)obj;
        return HealthRiskId == other.HealthRiskId && HealthRiskName == other.HealthRiskName
            && Data.All(period => other.Data.Any(period.ValueEquals));
    }
}
