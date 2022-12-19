using System.Collections.Generic;
using RX.Nyss.Web.Services.EidsrClient.Dto;

namespace RX.Nyss.Common.Services.EidsrClient.Dto;

public class EidsrRegisterEventRequest
{
    public EidsrRegisterEventRequestBody EidsrRegisterEventRequestBody { get; set; }

    public EidsrApiProperties EidsrApiProperties { get; set; }

    private EidsrRegisterEventRequest()
    {
    }

    public static EidsrRegisterEventRequest CreateEidsrRegisterEventRequest(
        EidsrRegisterEventRequestTemplate template,
        EidsrRegisterEventRequestData data)
    {
        var res = new EidsrRegisterEventRequest {
            EidsrApiProperties = template.EidsrApiProperties,
            EidsrRegisterEventRequestBody = new EidsrRegisterEventRequestBody {
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

        AddDataElement(dataValues, template.LocationDataElementId, data.Location);
        AddDataElement(dataValues, template.DateOfOnsetDataElementId, data.DateOfOnset);
        AddDataElement(dataValues, template.PhoneNumberDataElementId, data.PhoneNumber);
        AddDataElement(dataValues, template.SuspectedDiseaseDataElementId, data.SuspectedDisease);
        AddDataElement(dataValues, template.GenderDataElementId, data.Gender);
        AddDataElement(dataValues, template.EventTypeDataElementId, data.EventType);

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

