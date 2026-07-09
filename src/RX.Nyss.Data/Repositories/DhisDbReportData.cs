using NetTopologySuite.Geometries;

namespace RX.Nyss.Data.Repositories;

public class DhisDbReportData
{
    public string? OrgUnit { get; set; }

    public string? EventDate { get; set; }

    public string? ReportLocation { get; set; }

    public Point? ReportGeoLocation { get; set; }

    public string? ReportHealthRisk { get; set; }

    public string? ReportSuspectedDisease { get; set; }

    public string? ReportStatus { get; set; }

    public string? ReportGender { get; set; }

    public string? ReportAgeGroup { get; set; }

    public int ReportCaseCountFemaleAgeAtLeastFive { get; set; }

    public int ReportCaseCountMaleAgeAtLeastFive { get; set; }

    public int ReportCaseCountFemaleAgeBelowFive { get; set; }

    public int ReportCaseCountMaleAgeBelowFive { get; set; }

    public string? ReportDate { get; set; }

    public string? ReportTime { get; set; }

    public int? ReportDataCollectorId { get; set; }
}