using System.Collections.Generic;
using RX.Nyss.Web.Services.EidsrClient.Dto;

namespace RX.Nyss.Common.Services.DhisClient.Dto;
public class DhisRegisterReportRequest
{
    public DhisRegisterReportRequestBody DhisRegisterReportRequestBody { get; set; }

    public EidsrApiProperties EidsrApiProperties { get; set; }

    private DhisRegisterReportRequest()
    {
    }

    public static DhisRegisterReportRequest CreateDhisRegisterReportRequest(
        DhisRegisterReportRequestTemplate template,
        DhisRegisterReportRequestData data)
    {
        var res = new DhisRegisterReportRequest
        {
            EidsrApiProperties = template.EidsrApiProperties,
            DhisRegisterReportRequestBody = new DhisRegisterReportRequestBody
            {
                Program = template.Program,
                EventDate = data.EventDate,
                OrgUnit = data.OrgUnit,
                DataValues = GetDataValues(template, data)
            }
        };

        return res;
    }

    private static List<DhisRegisterReportRequestBody.DataValue> GetDataValues(
        DhisRegisterReportRequestTemplate template,
        DhisRegisterReportRequestData data)
    {
        var dataValues = new List<DhisRegisterReportRequestBody.DataValue>();

        AddDataElement(dataValues, template.ReportLocationDataElementId, data.ReportLocation);
        AddDataElement(dataValues, template.ReportSuspectedDiseaseDataElementId, data.ReportSuspectedDisease);
        AddDataElement(dataValues, template.ReportHealthRiskDataElementId, data.ReportHealthRisk);
        AddDataElement(dataValues, template.ReportStatusDataElementId, data.ReportStatus);
        AddDataElement(dataValues, template.ReportGenderDataElementId, data.ReportGender);
        AddDataElement(dataValues, template.ReportAgeAtLeastFiveDataElementId, data.ReportAgeAtleastFive);
        AddDataElement(dataValues, template.ReportAgeBelowFiveDataElementId, data.ReportAgeBelowFive);

        return dataValues;
    }

    private static void AddDataElement(List<DhisRegisterReportRequestBody.DataValue> list, string dataElementId, string value)
    {
        if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(dataElementId))
        {
            list.Add(new DhisRegisterReportRequestBody.DataValue
            {
                DataElement = dataElementId,
                Value = value,
            });
        }
    }
}
