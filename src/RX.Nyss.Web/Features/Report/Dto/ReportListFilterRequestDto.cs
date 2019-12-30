namespace RX.Nyss.Web.Features.Report.Dto
{
    public class ReportListFilterRequestDto
    {
        public ReportListType ReportListType { get; set; } = ReportListType.Main;

        public bool IsTraining { get; set; }
    }
}
