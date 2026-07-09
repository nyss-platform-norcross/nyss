using System.Collections.Generic;
using RX.Nyss.Web.Services.EidsrClient.Dto;

namespace RX.Nyss.Common.Services.EidsrClient.Dto;

public class EidsrRegisterEventRequest
{
    public EidsrRegisterEventRequestBody EidsrRegisterEventRequestBody { get; set; }

    public EidsrApiProperties EidsrApiProperties { get; set; }

    private EidsrRegisterEventRequest()
    {
        EidsrApiProperties = new EidsrApiProperties();
        EidsrRegisterEventRequestBody = new EidsrRegisterEventRequestBody();
    }

    public static EidsrRegisterEventRequest CreateEidsrRegisterEventRequest(
        EidsrRegisterEventRequestTemplate template,
        EidsrRegisterEventRequestData data)
    {
        var res = new EidsrRegisterEventRequest
        {
            EidsrApiProperties = template.EidsrApiProperties ?? new EidsrApiProperties(),
            EidsrRegisterEventRequestBody = new EidsrRegisterEventRequestBody
            {
                Program = template.Program,
                EventDate = data.EventDate,
                OrgUnit = data.OrgUnit,
                DataValues = GetDataValues(template, data)
            }
        };

        return res;
    }

    private static List<EidsrRegisterEventRequestBody.DataValue> GetDataValues(
        EidsrRegisterEventRequestTemplate template,
        EidsrRegisterEventRequestData data)
    {
        var dataValues = new List<EidsrRegisterEventRequestBody.DataValue>();

        AddDataElement(dataValues, template.LocationDataElementId ?? string.Empty, data.Location ?? string.Empty);
        AddDataElement(dataValues, template.DateOfOnsetDataElementId ?? string.Empty, data.DateOfOnset ?? string.Empty);
        AddDataElement(dataValues, template.PhoneNumberDataElementId ?? string.Empty, data.PhoneNumber ?? string.Empty);
        AddDataElement(dataValues, template.SuspectedDiseaseDataElementId ?? string.Empty, data.SuspectedDisease ?? string.Empty);
        AddDataElement(dataValues, template.GenderDataElementId ?? string.Empty, data.Gender ?? string.Empty);
        AddDataElement(dataValues, template.EventTypeDataElementId ?? string.Empty, data.EventType ?? string.Empty);

        return dataValues;
    }

    private static void AddDataElement(List<EidsrRegisterEventRequestBody.DataValue> list, string dataElementId, string value)
    {
        if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(dataElementId))
        {
            list.Add(new EidsrRegisterEventRequestBody.DataValue
            {
                DataElement = dataElementId,
                Value = value,
            });
        }
    }
}

