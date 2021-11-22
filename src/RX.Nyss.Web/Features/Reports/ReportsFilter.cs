using System;
using System.Collections.Generic;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.Reports
{
    public class ReportsFilter
    {
        public List<int> HealthRisks { get; set; }

        public int? NationalSocietyId { get; set; }

        public int? ProjectId { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public Area Area { get; set; }

        public int? OrganizationId { get; set; }

        public DataCollectorType? DataCollectorType { get; set; }

        public ReportStatusFilterDto ReportStatus { get; set; }

        public TrainingStatusDto? TrainingStatus { get; set; }

        public int UtcOffset { get; set; }
    }
}
