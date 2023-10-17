using RX.Nyss.Web.Services.EidsrClient.Dto;

namespace RX.Nyss.Common.Services.DhisClient.Dto;
public class DhisRegisterReportRequestTemplate
{
    public string Program { get; set; }

    public EidsrApiProperties EidsrApiProperties { get; set; }

    public string LocationDataElementId { get; set; }
    
    public string SuspectedDiseaseDataElementId { get; set; }

    public string HealthRiskDataElementId { get; set; }

    public string ReportStatusDataElementId { get; set; }

    public string GenderDataElementId { get; set; }

    public string AgeAtleastFiveDataElementId { get; set; }

    public string AgeBelowFiveDataElementId { get; set; }
}
