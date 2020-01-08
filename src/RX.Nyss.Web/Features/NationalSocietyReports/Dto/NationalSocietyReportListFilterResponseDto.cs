using System.Collections.Generic;

namespace RX.Nyss.Web.Features.NationalSocietyReports.Dto
{
    public class NationalSocietyReportListFilterResponseDto
    {
        public IEnumerable<HealthRiskDto> HealthRisks { get; set; }

        public class HealthRiskDto
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
