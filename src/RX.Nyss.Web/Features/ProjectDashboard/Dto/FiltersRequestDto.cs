using System;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Services.ReportsDashboard;

namespace RX.Nyss.Web.Features.ProjectDashboard.Dto
{
    public class FiltersRequestDto
    {
        public int? HealthRiskId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public AreaDto Area { get; set; }

        public DatesGroupingType GroupingType { get; set; }

        public ReportsTypeDto ReportsType { get; set; }

        public bool IsTraining { get; set; }

        public enum ReportsTypeDto
        {
            All,
            DataCollector,
            DataCollectionPoint
        }
    }
}
