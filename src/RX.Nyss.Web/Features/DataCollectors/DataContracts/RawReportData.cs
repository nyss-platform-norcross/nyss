using System;
using RX.Nyss.Common.Utils;

namespace RX.Nyss.Web.Features.DataCollectors.DataContracts
{
    public class RawReportData
    {
        public bool IsValid { get; set; }
        public bool DcIsTranining { get; set; }
        public EpiDate EpiDate { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}
