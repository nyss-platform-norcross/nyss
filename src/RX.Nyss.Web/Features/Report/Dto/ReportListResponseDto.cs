using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RX.Nyss.Web.Features.Report.Dto
{
    public class ReportListResponseDto
    {
        public DateTime CreatedAt { get; set; }
        public string HealthRiskName { get; set; }
        public string Status { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string Village { get; set; }
        public string Zone { get; set; }
        public string DataCollectorName { get; set; }
        public string DataCollectorPhoneNumber { get; set; }
        public int? CountMalesBelowFive { get; set; }
        public int? CountFemalesBelowFive { get; set; }
        public int? CountMaleAtLeastFive { get; set; }
        public int? CountFemalesAtLeastFive { get; set; }
    }
}
