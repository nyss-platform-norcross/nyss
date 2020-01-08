using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.NationalSocietyReports.Dto
{
    public class NationalSocietyReportListFilterRequestDto
    {
        public NationalSocietyReportListType ReportsType { get; set; } = NationalSocietyReportListType.Main;
        public AreaDto Area { get; set; }
        public int? HealthRiskId { get; set; }
        public bool Status { get; set; }
    }
}
