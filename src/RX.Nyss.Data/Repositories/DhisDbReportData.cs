using System;

namespace RX.Nyss.Data.Repositories;

public class DhisDbReportData
{
    public string OrgUnit { get; set; }

    public string EventDate { get; set; }

    public string ReportLocation	{ get; set; }

    public string ReportHealthRisk { get; set; }

    public string ReportSuspectedDisease	{ get; set; }

    public string ReportStatus { get; set; }

    public string ReportGender { get; set; }

    public string ReportAgeAtLeastFive { get; set; }

    public string ReportAgeBelowFive { get; set; }
}