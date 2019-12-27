using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Common
{
    public static class StatusConstants
    {
        public static readonly List<ReportStatus> ReportStatusesConsideredForAlertProcessing = new List<ReportStatus> { ReportStatus.Pending, ReportStatus.New, ReportStatus.Accepted };

        public static readonly List<AlertStatus> AlertStatusesAllowingCrossChecks = new List<AlertStatus> { AlertStatus.Pending, AlertStatus.Rejected, AlertStatus.Dismissed, AlertStatus.Escalated };
    };
}
