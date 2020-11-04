using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.Alerts.Dto
{
    public class AlertListFilterRequestDto
    {
        public const string TimeTriggeredColumnName = "TimeTriggered";
        public const string TimeOfLastReportColumnName = "TimeOfLastReport";
        public const string StatusColumnName = "Status";

        public AreaDto Area { get; set; }
        public int? HealthRiskId { get; set; }
        public AlertStatusFilter Status { get; set; }
        public string OrderBy { get; set; }
        public bool SortAscending { get; set; }
    }
}
