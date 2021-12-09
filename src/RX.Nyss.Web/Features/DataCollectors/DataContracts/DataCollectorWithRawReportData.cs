using System;
using System.Collections.Generic;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.DataCollectors.DataContracts
{
    public class DataCollectorWithRawReportData
    {
        public string Name { get; set; }
        public string VillageName { get; set; }
        public string PhoneNumber { get; set; }
        public IEnumerable<RawReportData> ReportsInTimeRange { get; set; }
        public DateTime CreatedAt { get; set; }
        public IEnumerable<DataCollectorNotDeployed> DatesNotDeployed { get; set; }
    }
}
