namespace RX.Nyss.Web.Features.Reports.Dto
{
    public class ExportCorrectReportListCsvContentDto
    {
        public int Id { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public int EpiWeek { get; set; }
        public int EpiYear { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Village { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
        public string HealthRiskName { get; set; } = string.Empty;
        public int? CountMalesBelowFive { get; set; }
        public int? CountMalesAtLeastFive { get; set; }
        public int? CountFemalesBelowFive { get; set; }
        public int? CountFemalesAtLeastFive { get; set; }
        public int? TotalBelowFive { get; set; }
        public int? TotalAtLeastFive { get; set; }
        public int? TotalMale { get; set; }
        public int? TotalFemale { get; set; }
        public int? Total { get; set; }
        public string DataCollectorDisplayName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? ReportAlertId { get; set; }
        public string Location { get; set; }    = string.Empty;
        public string Corrected { get; set; }   = string.Empty;
    }
}
