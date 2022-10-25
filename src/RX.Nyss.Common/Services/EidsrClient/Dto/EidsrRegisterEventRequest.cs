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
        var dataValues = new List<EidsrRegisterEventRequestBody.DataValue>
        {
            new()
            {
                DataElement = template.LocationDataElementId,
                Value = data.Location,
            },
            new()
            {
                DataElement = template.DateOfOnsetDataElementId,
                Value = data.DateOfOnset,
            },
            new()
            {
                DataElement = template.DateOfOnsetDataElementId,
                Value = data.DateOfOnset,
            },
            new()
            {
                DataElement = template.PhoneNumberDataElementId,
                Value = data.PhoneNumber,
            },
            new()
            {
                DataElement = template.SuspectedDiseaseDataElementId,
                Value = data.SuspectedDisease,
            },
            new()
            {
                DataElement = template.GenderDataElementId,
                Value = data.Gender,
            }
        };

        return dataValues;
    }
}

