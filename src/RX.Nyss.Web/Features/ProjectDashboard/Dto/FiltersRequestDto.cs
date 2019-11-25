using System;

namespace RX.Nyss.Web.Features.ProjectDashboard.Dto
{
    public class FiltersRequestDto
    {
        public int? HealthRiskId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public AreaDto Area { get; set; }

        public class AreaDto
        {
            public int? RegionId { get; set; }

            public int? DistrictId { get; set; }

            public int? VillageId { get; set; }

            public int? ZoneId { get; set; }
        }
    }
}
