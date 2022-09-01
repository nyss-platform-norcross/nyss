namespace RX.Nyss.Data.Models;

public class EidsrConfiguration
{
    public int Id { get; set; }

    public string Username { get; set; }

    public string PasswordHash { get; set; }

    public string ApiBaseUrl { get; set; }

    public string TrackerProgramId { get; set; }

    public string LocationDataElementId	{ get; set; }

    public string DateOfOnsetDataElementId { get; set; }

    public string PhoneNumberDataElementId { get; set; }

    public string SuspectedDiseaseDataElementId	{ get; set; }

    public string EventTypeDataElementId { get; set; }

    public string GenderDataElementId { get; set; }

    public int NationalSocietyId { get; set; }

    public virtual NationalSociety NationalSociety { get; set; }
}
