namespace RX.Nyss.Web.Features.Report.Dto
{
    public class ListFilterRequestDto
    {
        public ReportListTypeDto ReportListType { get; set; } = ReportListTypeDto.Main;

        public bool IsTraining { get; set; }
    }
    public enum ReportListTypeDto
    {
        Main,
        FromDcp
    }
}
