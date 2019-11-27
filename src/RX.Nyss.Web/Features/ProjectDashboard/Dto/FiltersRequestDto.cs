using System;

namespace RX.Nyss.Web.Features.ProjectDashboard.Dto
{
    public class FiltersRequestDto
    {
        public int? HealthRiskId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public AreaDto Area { get; set; }

        public GroupingTypeDto GroupingType { get; set; }

        public class AreaDto
        {
            public AreaTypeDto Type { get; set; }

            public int Id { get; set; }
        }

        public enum GroupingTypeDto
        {
            Day,
            Week
        }

        public enum AreaTypeDto
        {
            Region,
            District,
            Village,
            Zone
        }
    }
}
