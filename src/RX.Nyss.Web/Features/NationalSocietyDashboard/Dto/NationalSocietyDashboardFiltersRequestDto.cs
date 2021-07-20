using System;
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

        public int? HealthRiskId { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public AreaDto Area { get; set; }

        public int? OrganizationId { get; set; }

        public DatesGroupingType GroupingType { get; set; }

        public NationalSocietyDataCollectorTypeDto DataCollectorType { get; set; }

        public ReportStatusFilterDto ReportStatus { get; set; }

        public int UtcOffset { get; set; }
    }
}
