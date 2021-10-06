using System;
using System.Collections.Generic;

namespace RX.Nyss.Web.Features.DataCollectors.DataContracts
{
    public class DataCollectorWithRawReportDataForExport
    {
        public string Name { get; set; }
        public string VillageName { get; set; }
        public string PhoneNumber { get; set; }
        public IEnumerable<RawReportDataForExport> ReportsInTimeRange { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
