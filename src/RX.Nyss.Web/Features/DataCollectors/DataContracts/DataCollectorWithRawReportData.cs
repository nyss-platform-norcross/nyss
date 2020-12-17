using System.Collections.Generic;

namespace RX.Nyss.Web.Features.DataCollectors.DataContracts
{
    public class DataCollectorWithRawReportData
    {
        public string Name { get; set; }
        public string VillageName { get; set; }
        public string PhoneNumber { get; set; }
        public IEnumerable<RawReportData> ReportsInTimeRange { get; set; }
    }
}
