using System;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Reports.Dto
{
    public interface IReportListResponseDto
    {
        int Id { get; set; }
        DateTime DateTime { get; set; }
        string Region { get; set; }
        string District { get; set; }
        string Village { get; set; }
        string Zone { get; set; }
        string DataCollectorDisplayName { get; set; }
        string PhoneNumber { get; set; }
        bool IsAnonymized { get; set; }
        string OrganizationName { get; set; }
        string SupervisorName { get; set; }
    }

    public class ReportListResponseDto : IReportListResponseDto
    {
        public int Id { get; set; }
        public string HealthRiskName { get; set; }
        public bool IsValid { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string Village { get; set; }
        public string Zone { get; set; }
        public string DataCollectorDisplayName { get; set; }
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public int? CountMalesBelowFive { get; set; }
        public int? CountFemalesBelowFive { get; set; }
        public int? CountMalesAtLeastFive { get; set; }
        public int? CountFemalesAtLeastFive { get; set; }
        public int? ReferredCount { get; set; }
        public int? DeathCount { get; set; }
        public int? FromOtherVillagesCount { get; set; }
        public bool? IsMarkedAsError { get; set; }
        public int? ReportId { get; set; }
        public ReportType? ReportType { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsAnonymized { get; set; }
        public string OrganizationName { get; set; }
        public string SupervisorName { get; set; }
        public bool IsActivityReport { get; set; }
        public ReportStatus Status { get; set; }
        public ReportListAlert Alert { get; set; }
        public ReportErrorType? ReportErrorType { get; set; }
        public bool DataCollectorIsDeleted { get; set; }
    }

    public class ReportListAlert
    {
        public int Id { get; set; }
        public AlertStatus Status { get; set; }
        public bool ReportWasCrossCheckedBeforeEscalation { get; set; }
    }
}
