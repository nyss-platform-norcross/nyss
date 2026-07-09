namespace RX.Nyss.Data.Concepts
{
    public static class StatusConstants
    {
        public static readonly ReportStatus[] ReportStatusesConsideredForAlertProcessing = { ReportStatus.Pending, ReportStatus.New, ReportStatus.Accepted };

        public static readonly ReportStatus[] ReportStatusesAllowedToBeReset = { ReportStatus.Accepted, ReportStatus.Rejected };

        public static readonly AlertStatus[] AlertStatusesAllowingCrossChecks = { AlertStatus.Open, AlertStatus.Escalated };

        public static readonly AlertStatus[] AlertStatusesNotAllowingReportsToTriggerNewAlert = { AlertStatus.Escalated, AlertStatus.Closed };
    }
}
