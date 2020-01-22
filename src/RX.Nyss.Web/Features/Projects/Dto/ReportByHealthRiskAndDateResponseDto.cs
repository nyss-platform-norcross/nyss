using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Projects.Dto
{
    public class ReportByHealthRiskAndDateResponseDto
    {
        public IEnumerable<HealthRiskDto> HealthRisks { get; set; } = new List<HealthRiskDto>();

        public IEnumerable<string> AllPeriods { get; set; } = new List<string>();


        public class HealthRiskDto
        {
            public string HealthRiskName { get; set; }

            public IEnumerable<PeriodDto> Periods { get; set; }
        }
    }
}
