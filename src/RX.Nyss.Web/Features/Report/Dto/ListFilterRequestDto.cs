namespace RX.Nyss.Web.Features.Report.Dto
{
    public class ListFilterRequestDto
    {
        public ReportListType ReportListType { get; set; } = ReportListType.Main;

        public bool IsTraining { get; set; }
    }

    public enum ReportListType
    {
        Main,
        FromDcp
    }
}
