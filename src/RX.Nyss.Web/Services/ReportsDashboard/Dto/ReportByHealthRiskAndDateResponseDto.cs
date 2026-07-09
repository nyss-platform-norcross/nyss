using System.Collections.Generic;

namespace RX.Nyss.Web.Services.ReportsDashboard.Dto
{
    public class ReportByHealthRiskAndDateResponseDto
    {
        public IEnumerable<ReportHealthRiskDto> HealthRisks { get; set; } = new List<ReportHealthRiskDto>();

        public IEnumerable<string> AllPeriods { get; set; } = new List<string>();


        public class ReportHealthRiskDto
        {
            public string? HealthRiskName { get; set; }

            public IEnumerable<PeriodDto>? Periods { get; set; }
        }
    }
}
