using System;
using System.Collections.Generic;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.DataCollectors.DataContracts
{
    public class DataCollectorWithRawReportData
    {
        public string Name { get; set; } = string.Empty;
        public string VillageName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public IEnumerable<RawReportData> ReportsInTimeRange { get; set; } = new List<RawReportData>();
        public DateTime CreatedAt { get; set; }
        public IEnumerable<DataCollectorNotDeployed> DatesNotDeployed { get; set; } = new List<DataCollectorNotDeployed>();
        public DayOfWeek EpiWeekStartDay { get; set; }
    }
}
