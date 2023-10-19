using System.ComponentModel.DataAnnotations.Schema;

namespace RX.Nyss.Data.Models;

public class EidsrConfiguration
{
    public int Id { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string Username { get; set; }

    public string PasswordHash { get; set; }

    public string ApiBaseUrl { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string TrackerProgramId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string LocationDataElementId	{ get; set; }

    [Column(TypeName = "varchar(256)")]
    public string DateOfOnsetDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string PhoneNumberDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string SuspectedDiseaseDataElementId	{ get; set; }

    [Column(TypeName = "varchar(256)")]
    public string EventTypeDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string GenderDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string ReportLocationDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string ReportHealthRiskDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string ReportSuspectedDiseaseDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string ReportStatusDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string ReportGenderDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string ReportAgeAtLeastFiveDataElementId { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string ReportAgeBelowFiveDataElementId { get; set; }

    public int NationalSocietyId { get; set; }

    public virtual NationalSociety NationalSociety { get; set; }
}