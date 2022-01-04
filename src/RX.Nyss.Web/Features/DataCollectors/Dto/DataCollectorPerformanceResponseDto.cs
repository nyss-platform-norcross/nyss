using System.Collections.Generic;
using RX.Nyss.Common.Utils;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class DataCollectorPerformanceResponseDto
    {
        public IList<Completeness> Completeness { get; set; }
        public PaginatedList<DataCollectorPerformance> Performance { get; set; }
        public IList<EpiDate> EpiDateRange { get; set; }
    }

    public class DataCollectorPerformance
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string VillageName { get; set; }
        public string DistrictName { get; set; }
        public string RegionName { get; set; }
        public int? DaysSinceLastReport { get; set; }
        public List<PerformanceInEpiWeek> PerformanceInEpiWeeks { get; set; }
    }

    public class PerformanceInEpiWeek
    {
        public int EpiWeek { get; set; }
        public int EpiYear { get; set; }
        public ReportingStatus? ReportingStatus { get; set; }
    }

    public class Completeness
    {
        public int EpiWeek { get; set; }
        public int EpiYear { get; set; }
        public double ActiveDataCollectors { get; set; }
        public double TotalDataCollectors { get; set; }
        public int Percentage { get; set; }
    }
}
