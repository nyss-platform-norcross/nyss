using System.ComponentModel.DataAnnotations.Schema;

namespace RX.Nyss.Data.Models;

public class EidsrConfiguration
{
    public int Id { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string? Username { get; set; }

    public string? PasswordHash { get; set; }

    public string? ApiBaseUrl { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string? TrackerProgramId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string? LocationDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string? DateOfOnsetDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string? PhoneNumberDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string? SuspectedDiseaseDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string? EventTypeDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string? GenderDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string? ReportLocationDataElementId { get; set; }

    public string? ReportGeoLocationDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string? ReportHealthRiskDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string? ReportSuspectedDiseaseDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
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

    public int NationalSocietyId { get; set; }

    public virtual NationalSociety? NationalSociety { get; set; }
}