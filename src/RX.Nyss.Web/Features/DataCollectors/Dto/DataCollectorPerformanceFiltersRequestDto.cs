using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class DataCollectorPerformanceFiltersRequestDto
    {
        public AreaDto Area { get; set; }

        public string Name { get; set; }

        public int? SupervisorId { get; set; }

        public TrainingStatusDto TrainingStatus { get; set; }

        public IEnumerable<PerformanceStatusFilterDto> EpiWeekFilters { get; set; }

        public int PageNumber { get; set; }
    }

    public class PerformanceStatusFilterDto
    {
        public int EpiWeek { get; set; }
        public bool ReportingCorrectly { get; set; }

        public bool ReportingWithErrors { get; set; }

        public bool NotReporting { get; set; }
    }
}
