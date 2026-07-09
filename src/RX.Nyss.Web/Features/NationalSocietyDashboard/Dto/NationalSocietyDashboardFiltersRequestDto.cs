using System;
using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Services.ReportsDashboard;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard.Dto
{
    public class NationalSocietyDashboardFiltersRequestDto
    {
        public enum NationalSocietyDataCollectorTypeDto
        {
            All,
            DataCollector,
            DataCollectionPoint
        }

        public IEnumerable<int> HealthRisks { get; set; } = new List<int>();

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public AreaDto Locations { get; set; } = new AreaDto();

        public int? OrganizationId { get; set; }

        public DatesGroupingType GroupingType { get; set; }

        public NationalSocietyDataCollectorTypeDto DataCollectorType { get; set; }

        public ReportStatusFilterDto ReportStatus { get; set; } = new ReportStatusFilterDto();

        public int UtcOffset { get; set; }
    }
}
