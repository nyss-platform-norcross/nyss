using System.Linq;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Services.ReportsDashboard.Dto;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard.Dto
{
    public class NationalSocietySummaryResponseDto
    {
        public int KeptReportCount { get; set; }
        public int DismissedReportCount { get; set; }
        public int NotCrossCheckedReportCount { get; set; }
        public int TotalReportCount { get; set; }
        public int ActiveDataCollectorCount { get; set; }
        public DataCollectionPointsSummaryResponse DataCollectionPointSummary { get; set; } = new DataCollectionPointsSummaryResponse();
        public AlertsSummaryResponseDto AlertsSummary { get; set; } = new AlertsSummaryResponseDto();
        public int NumberOfDistricts { get; set; }
        public int NumberOfVillages { get; set; }
    }
}
