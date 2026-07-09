using RX.Nyss.Web.Services.EidsrClient.Dto;

namespace RX.Nyss.Common.Services.DhisClient.Dto;
public class DhisRegisterReportRequestTemplate
{
    public string? Program { get; set; }

    public EidsrApiProperties? EidsrApiProperties { get; set; }

    public string? ReportLocationDataElementId { get; set; }

    public string? ReportGeoLocationDataElementId { get; set; }

    public string? ReportSuspectedDiseaseDataElementId { get; set; }

    public string? ReportHealthRiskDataElementId { get; set; }

    public string? ReportStatusDataElementId { get; set; }

    public string? ReportGenderDataElementId { get; set; }

    public string? ReportAgeGroupDataElementId { get; set; }

    public string? ReportCaseCountFemaleAgeAtLeastFiveDataElementId { get; set; }

    public string? ReportCaseCountMaleAgeAtLeastFiveDataElementId { get; set; }

    public string? ReportCaseCountFemaleAgeBelowFiveDataElementId { get; set; }

    public string? ReportCaseCountMaleAgeBelowFiveDataElementId { get; set; }

    public string? ReportDateDataElementId { get; set; }

    public string? ReportTimeDataElementId { get; set; }

    public string? ReportDataCollectorIdDataElementId { get; set; }
}
