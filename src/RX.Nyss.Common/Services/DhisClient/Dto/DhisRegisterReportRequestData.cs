
namespace RX.Nyss.Common.Services.DhisClient.Dto;
public class DhisRegisterReportRequestData
{
    public string OrgUnit { get; set; }

    public string EventDate { get; set; }

    public string ReportLocation { get; set; }

    public string ReportSuspectedDisease { get; set; }

    public string ReportHealthRisk { get; set; }

    public string ReportStatus { get; set; }

    public string ReportGender { get; set; }

    public string ReportAgeAtleastFive { get; set; }

    public string ReportAgeBelowFive { get; set; }
}
