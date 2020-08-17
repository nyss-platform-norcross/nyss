using System;
using NetTopologySuite.Geometries;

namespace RX.Nyss.Web.Features.Reports.Dto
{
    public class ExportReportListResponseDto : IReportListResponseDto
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public Point Location { get; set; }
        public string HealthRiskName { get; set; }
        public bool IsValid { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string Village { get; set; }
        public string Zone { get; set; }
        public string DataCollectorDisplayName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsAnonymized { get; set; }
        public string OrganizationName { get; set; }
        public string SupervisorName { get; set; }
        public string Message { get; set; }
        public int? CountMalesBelowFive { get; set; }
        public int? CountFemalesBelowFive { get; set; }
        public int? CountMalesAtLeastFive { get; set; }
        public int? CountFemalesAtLeastFive { get; set; }
        public int? ReferredCount { get; set; }
        public int? DeathCount { get; set; }
        public int? FromOtherVillagesCount { get; set; }
        public int EpiWeek { get; set; }
        public int EpiYear { get; set; }
        public bool MarkedAsError { get; set; }
        public DateTime DateTime { get; set; }
    }
}
