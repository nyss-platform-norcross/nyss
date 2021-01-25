using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.Reports.Dto
{
    public class ReportListFilterRequestDto
    {
        public static readonly string DateColumnName = "date";
        public ReportListType ReportsType { get; set; } = ReportListType.Main;
        public AreaDto Area { get; set; }
        public int? HealthRiskId { get; set; }
        public bool Status { get; set; }
        public bool IsTraining { get; set; }
        public string OrderBy { get; set; }
        public bool SortAscending { get; set; }
        public int UtcOffset { get; set; }
    }
}
