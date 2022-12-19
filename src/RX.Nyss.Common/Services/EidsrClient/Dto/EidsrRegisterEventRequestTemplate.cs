using RX.Nyss.Web.Services.EidsrClient.Dto;

namespace RX.Nyss.Common.Services.EidsrClient.Dto;

public class EidsrRegisterEventRequestTemplate
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