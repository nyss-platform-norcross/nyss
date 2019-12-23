namespace RX.Nyss.Web.Features.Report.Dto
{
    public class ListFilterRequestDto
    {
        public ReportListTypeDto ReportListType { get; set; } = ReportListTypeDto.Main;
    }
    public enum ReportListTypeDto
    {
        Main,
        Training,
        FromDcp
    }
}
