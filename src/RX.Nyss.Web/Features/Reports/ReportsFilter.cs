using System;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;

namespace RX.Nyss.Web.Features.Reports
{
    public class ReportsFilter
    {
        public int? HealthRiskId { get; set; }

        public int? NationalSocietyId { get; set; }

        public int? ProjectId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public Area Area { get; set; }

        public int? OrganizationId { get; set; }

        public DataCollectorType? DataCollectorType { get; set; }

        public bool IsTraining { get; set; }
    }
}
