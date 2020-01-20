using System.Collections.Generic;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard.Dto
{
    public class NationalSocietyDashboardResponseDto
    {
        public NationalSocietySummaryResponseDto Summary { get; set; }

        public IEnumerable<NationalSocietySummaryMapResponseDto> ReportsGroupedByLocation { get; set; }
    }
}
