using System.Collections.Generic;

namespace RX.Nyss.Web.Services.ReportsDashboard.Dto
{
    public class ReportByVillageAndDateResponseDto
    {
        public IEnumerable<VillageDto> Villages { get; set; } = new List<VillageDto>();

        public IEnumerable<string> AllPeriods { get; set; } = new List<string>();

        public class VillageDto
        {
            public string? Name { get; set; }

            public IEnumerable<PeriodDto>? Periods { get; set; }
        }
    }
}
