using System;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard.Dto
{
    public class NationalSocietyDashboardFiltersRequestDto
    {
        public int? HealthRiskId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public AreaDto Area { get; set; }

        public GroupingTypeDto GroupingType { get; set; }

        public ReportsTypeDto ReportsType { get; set; }

        public bool IsTraining { get; set; }

        public enum GroupingTypeDto
        {
            Day,
            Week
        }

        public enum ReportsTypeDto
        {
            All,
            DataCollector,
            DataCollectionPoint
        }
    }
}
