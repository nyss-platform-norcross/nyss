namespace RX.Nyss.Data.Concepts
{
    public static class StatusConstants
    {
        public static readonly ReportStatus[] ReportStatusesConsideredForAlertProcessing = { ReportStatus.Pending, ReportStatus.New, ReportStatus.Accepted };

        public static readonly AlertStatus[] AlertStatusesAllowingCrossChecks = { AlertStatus.Pending, AlertStatus.Rejected, AlertStatus.Dismissed, AlertStatus.Escalated };
    }
}
