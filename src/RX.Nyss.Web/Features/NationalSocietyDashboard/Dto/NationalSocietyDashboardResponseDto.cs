using System.Collections.Generic;
using RX.Nyss.Web.Services.ReportsDashboard.Dto;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard.Dto
{
    public class NationalSocietyDashboardResponseDto
    {
        public NationalSocietySummaryResponseDto Summary { get; set; } = new NationalSocietySummaryResponseDto();

        public IEnumerable<ReportsSummaryMapResponseDto> ReportsGroupedByLocation { get; set; } = new List<ReportsSummaryMapResponseDto>();

        public ReportByVillageAndDateResponseDto ReportsGroupedByVillageAndDate { get; set; } = new ReportByVillageAndDateResponseDto();

        public ReportByHealthRiskAndDateResponseDto ReportsGroupedByHealthRiskAndDate { get; set; } = new ReportByHealthRiskAndDateResponseDto();

    }
}
