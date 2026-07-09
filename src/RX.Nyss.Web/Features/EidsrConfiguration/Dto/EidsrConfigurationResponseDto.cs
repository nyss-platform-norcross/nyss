using System.Collections.Generic;

namespace RX.Nyss.Web.Features.EidsrConfiguration.Dto;

public class EidsrIntegrationResponseDto
{
    public int? Id { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public string ApiBaseUrl { get; set; }

    public string TrackerProgramId { get; set; }

    public string LocationDataElementId { get; set; }

    public string DateOfOnsetDataElementId { get; set; }

    public string PhoneNumberDataElementId { get; set; }

    public string SuspectedDiseaseDataElementId { get; set; }

    public string EventTypeDataElementId { get; set; }

    public string GenderDataElementId { get; set; }

    public string ReportLocationDataElementId { get; set; }

    public string ReportGeoLocationDataElementId { get; set; }

    public string ReportHealthRiskDataElementId { get; set; }

    public string ReportSuspectedDiseaseDataElementId { get; set; }

    public string ReportStatusDataElementId { get; set; }

    public string ReportGenderDataElementId { get; set; }

    public string ReportAgeGroupDataElementId { get; set; }

    public string ReportCaseCountFemaleAgeAtLeastFiveDataElementId { get; set; }

    public string ReportCaseCountMaleAgeAtLeastFiveDataElementId { get; set; }

    public string ReportCaseCountFemaleAgeBelowFiveDataElementId { get; set; }

    public string ReportCaseCountMaleAgeBelowFiveDataElementId { get; set; }

    public string ReportDateDataElementId { get; set; }

    public string ReportTimeDataElementId { get; set; }

    public string ReportDataCollectorIdDataElementId { get; set; }

    public List<DistrictsWithOrganizationUnits> DistrictsWithOrganizationUnits { get; set; }
}