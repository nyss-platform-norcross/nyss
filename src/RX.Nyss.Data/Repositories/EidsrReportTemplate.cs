namespace RX.Nyss.Data.Repositories;

public class EidsrReportTemplate
{
    public string Program { get; set; }

    public EidsrApiProperties EidsrApiProperties { get; set; }

    public string LocationDataElementId	{ get; set; }

    public string DateOfOnsetDataElementId { get; set; }

    public string PhoneNumberDataElementId { get; set; }

    public string SuspectedDiseaseDataElementId	{ get; set; }

    public string EventTypeDataElementId { get; set; }

    public string GenderDataElementId { get; set; }
}