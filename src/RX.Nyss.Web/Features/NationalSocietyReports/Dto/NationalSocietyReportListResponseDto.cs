using System;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.NationalSocietyReports.Dto
{
    public class NationalSocietyReportListResponseDto
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string HealthRiskName { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public string Region { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Village { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
        public string DataCollectorDisplayName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? CountMalesBelowFive { get; set; }
        public int? CountFemalesBelowFive { get; set; }
        public int? CountMalesAtLeastFive { get; set; }
        public int? CountFemalesAtLeastFive { get; set; }
        public int? ReferredCount { get; set; }
        public int? DeathCount { get; set; }
        public int? FromOtherVillagesCount { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public bool IsAnonymized { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public bool IsActivityReport { get; set; }
        public ReportStatus Status { get; set; }
        public ReportErrorType? ErrorType { get; set; }
    }
}
