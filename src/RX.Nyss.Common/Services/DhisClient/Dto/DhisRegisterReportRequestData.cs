
namespace RX.Nyss.Common.Services.DhisClient.Dto;
public class DhisRegisterReportRequestData
{
    public string OrgUnit { get; set; }

    public string EventDate { get; set; }

    public string Location { get; set; }

    public string SuspectedDisease { get; set; }

    public string HealthRisk { get; set; }

    public string ReportStatus { get; set; }

    public string Gender { get; set; }

    public string AgeAtleastFive { get; set; }

    public string AgeBelowFive { get; set; }
}
