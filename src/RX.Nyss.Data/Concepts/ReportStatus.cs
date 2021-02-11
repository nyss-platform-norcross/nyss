namespace RX.Nyss.Data.Concepts
{
    public enum ReportStatus
    {
        New,        // Successful but not part of any alert and not cross checked
        Pending,    // Part of an alert, but not cross checked yet
        Rejected,   // Cross checked: Dismissed
        Accepted,   // Cross checked: Kept
        Closed      // Previously pending in an escalated alert that have been closed
    }
}
