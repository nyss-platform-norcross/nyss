using System;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.Reports.Dto
{
    public class ReportResponseDto
    {
        public int Id { get; set; }
        public int DataCollectorId { get; set; }
        public DataCollectorType DataCollectorType { get; set; }
        public DateTime Date { get; set; }
        public int HealthRiskId { get; set; }
        public int CountMalesBelowFive { get; set; }
        public int CountMalesAtLeastFive { get; set; }
        public int CountFemalesBelowFive { get; set; }
        public int CountFemalesAtLeastFive { get; set; }
        public int CountUnspecifiedSexAndAge { get; set; }
        public int ReferredCount { get; set; }
        public int DeathCount { get; set; }
        public int FromOtherVillagesCount { get; set; }
        public ReportType ReportType { get; set; }
        public ReportStatus ReportStatus { get; set; }
        public int LocationId { get; set; }
        public bool IsActivityReport { get; set; }
    }
}
