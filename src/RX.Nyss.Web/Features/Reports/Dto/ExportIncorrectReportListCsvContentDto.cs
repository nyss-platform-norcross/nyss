namespace RX.Nyss.Web.Features.Reports.Dto
{
    public class ExportIncorrectReportListCsvContentDto
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public int EpiWeek { get; set; }
        public int EpiYear { get; set; }
        public string Message { get; set; }
        public string ErrorType { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string Village { get; set; }
        public string Zone { get; set; }
        public string DataCollectorDisplayName { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public string Corrected { get; set; }
    }
}
