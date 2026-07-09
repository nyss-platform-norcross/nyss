namespace RX.Nyss.Web.Features.Reports.Dto
{
    public class ExportIncorrectReportListCsvContentDto
    {
        public int Id { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public int EpiWeek { get; set; }
        public int EpiYear { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorType { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Village { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
        public string DataCollectorDisplayName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Corrected { get; set; } = string.Empty;
    }
}
