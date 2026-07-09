using System;
using System.Collections.Generic;
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

        public IEnumerable<int> HealthRisks { get; set; } = new List<int>();

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public AreaDto Locations { get; set; } = new AreaDto();

        public DatesGroupingType GroupingType { get; set; }

        public DataCollectorTypeFilterDto DataCollectorType { get; set; }

        public ReportStatusFilterDto ReportStatus { get; set; } = new ReportStatusFilterDto();

        public TrainingStatusDto? TrainingStatus { get; set; }

        public int? OrganizationId { get; set; }

        public int UtcOffset { get; set; }
    }
}
