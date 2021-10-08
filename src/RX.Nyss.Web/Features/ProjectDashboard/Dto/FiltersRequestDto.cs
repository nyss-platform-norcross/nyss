using System;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Services.ReportsDashboard;

namespace RX.Nyss.Web.Features.ProjectDashboard.Dto
{
    public class FiltersRequestDto
    {
        public enum DataCollectorTypeFilterDto
        {
            All,
            DataCollector,
            DataCollectionPoint
        }

        public int? HealthRiskId { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public AreaDto Area { get; set; }

        public DatesGroupingType GroupingType { get; set; }

        public DataCollectorTypeFilterDto DataCollectorType { get; set; }

        public ReportStatusFilterDto ReportStatus { get; set; }

        public TrainingStatusDto? DataCollectorStatus { get; set; }

        public int? OrganizationId { get; set; }

        public int UtcOffset { get; set; }
    }
}
