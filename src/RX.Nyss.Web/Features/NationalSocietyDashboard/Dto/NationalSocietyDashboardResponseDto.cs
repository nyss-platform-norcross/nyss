using System.Collections.Generic;
using RX.Nyss.Web.Services.ReportsDashboard.Dto;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard.Dto
{
    // Defines the response object for getData in NationalSocietyDashboardService
    public class NationalSocietyDashboardResponseDto
    {
        public NationalSocietySummaryResponseDto Summary { get; set; } = new NationalSocietySummaryResponseDto();

        public IEnumerable<ReportsSummaryMapResponseDto> ReportsGroupedByLocation { get; set; } = new List<ReportsSummaryMapResponseDto>();

        public ReportByVillageAndDateResponseDto ReportsGroupedByVillageAndDate { get; set; } = new ReportByVillageAndDateResponseDto();

        public ReportByHealthRiskAndDateResponseDto ReportsGroupedByHealthRiskAndDate { get; set; } = new ReportByHealthRiskAndDateResponseDto();

        public IEnumerable<ReportByFeaturesAndDateResponseDto> ReportsGroupedByFeaturesAndDate { get; set; } = new List<ReportByFeaturesAndDateResponseDto>();

        public ReportByFeaturesAndDateResponseDto ReportsGroupedByFeatures { get; set; } = new ReportByFeaturesAndDateResponseDto();

        public ReportHistogramResponseDto KeptReportsInEscalatedAlertsHistogramData { get; set; } =
            new ReportHistogramResponseDto();

    }
}
