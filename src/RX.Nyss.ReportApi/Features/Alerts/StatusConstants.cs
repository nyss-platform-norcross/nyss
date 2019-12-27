using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.ReportApi.Features.Alerts
{
    public static class StatusConstants
    {
        public static readonly List<ReportStatus> ReportStatusesConsideredForAlertProcessing = new List<ReportStatus> { ReportStatus.Pending, ReportStatus.New, ReportStatus.Accepted };
    }
}
