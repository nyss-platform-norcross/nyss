using System.Collections.Generic;
using RX.Nyss.Web.Services.EidsrClient.Dto;

namespace RX.Nyss.Common.Services.DhisClient.Dto;
public class DhisRegisterReportRequest
{
    public DhisRegisterReportRequestBody DhisRegisterReportRequestBody { get; set; }

    public EidsrApiProperties EidsrApiProperties { get; set; }

    private DhisRegisterReportRequest()
    {
        DhisRegisterReportRequestBody = new DhisRegisterReportRequestBody();
        EidsrApiProperties = new EidsrApiProperties();
    }

    public static DhisRegisterReportRequest CreateDhisRegisterReportRequest(
        DhisRegisterReportRequestTemplate template,
        DhisRegisterReportRequestData data)
    {
        var res = new DhisRegisterReportRequest
        {
            /*EidsrApiProperties = template.EidsrApiProperties,
            DhisRegisterReportRequestBody = new DhisRegisterReportRequestBody
            {
                Program = template.Program,
                EventDate = data.EventDate,
                OrgUnit = data.OrgUnit,
                DataValues = GetDataValues(template, data)
            }*/
        };

        return res;
    }

    private static List<DhisRegisterReportRequestBody.DataValue> GetDataValues(
        DhisRegisterReportRequestTemplate template,
        DhisRegisterReportRequestData data)
    {
        var dataValues = new List<DhisRegisterReportRequestBody.DataValue>();

        if (template.ReportLocationDataElementId != "")
        {
            AddDataElement(dataValues, template.ReportLocationDataElementId ?? string.Empty, data.ReportLocation ?? string.Empty);
        }
        if (template.ReportGeoLocationDataElementId != "")
        {
            AddDataElement(dataValues, template.ReportGeoLocationDataElementId ?? string.Empty, data.ReportGeoLocation?.ToString() ?? string.Empty);
        }
        if (template.ReportSuspectedDiseaseDataElementId != "")
        {
            AddDataElement(dataValues, template.ReportSuspectedDiseaseDataElementId ?? string.Empty, data.ReportSuspectedDisease ?? string.Empty);
        }
        if (template.ReportHealthRiskDataElementId != "")
        {
            AddDataElement(dataValues, template.ReportHealthRiskDataElementId ?? string.Empty, data.ReportHealthRisk ?? string.Empty);
        }
        if (template.ReportStatusDataElementId != "")
        {
            AddDataElement(dataValues, template.ReportStatusDataElementId ?? string.Empty, data.ReportStatus ?? string.Empty);
        }
        if (template.ReportGenderDataElementId != "")
        {
            AddDataElement(dataValues, template.ReportGenderDataElementId ?? string.Empty, data.ReportGender ?? string.Empty);
        }
        if (template.ReportAgeGroupDataElementId != "")
        {
            AddDataElement(dataValues, template.ReportAgeGroupDataElementId ?? string.Empty, data.ReportAgeGroup ?? string.Empty);
        }
        if (template.ReportCaseCountFemaleAgeAtLeastFiveDataElementId != "")
        {
            AddDataElement(dataValues, template.ReportCaseCountFemaleAgeAtLeastFiveDataElementId ?? string.Empty, data.ReportCaseCountFemaleAgeAtLeastFive.ToString() ?? string.Empty);
        }
        if (template.ReportCaseCountMaleAgeAtLeastFiveDataElementId != "")
        {
            AddDataElement(dataValues, template.ReportCaseCountMaleAgeAtLeastFiveDataElementId ?? string.Empty, data.ReportCaseCountMaleAgeAtLeastFive.ToString());
        }
        if (template.ReportCaseCountFemaleAgeBelowFiveDataElementId != "")
        {
            AddDataElement(dataValues, template.ReportCaseCountFemaleAgeBelowFiveDataElementId ?? string.Empty, data.ReportCaseCountFemaleAgeBelowFive.ToString());
        }
        if (template.ReportCaseCountMaleAgeBelowFiveDataElementId != "")
        {
            AddDataElement(dataValues, template.ReportCaseCountMaleAgeBelowFiveDataElementId ?? string.Empty, data.ReportCaseCountMaleAgeBelowFive.ToString());
        }
        if (template.ReportDateDataElementId != "")
        {
            AddDataElement(dataValues, template.ReportDateDataElementId ?? string.Empty, data.ReportDate ?? string.Empty);
        }
        if (template.ReportTimeDataElementId != "")
        {
            AddDataElement(dataValues, template.ReportTimeDataElementId ?? string.Empty, data.ReportTime ?? string.Empty);
        }
        if (template.ReportDataCollectorIdDataElementId != "")
        {
            AddDataElement(dataValues, template.ReportDataCollectorIdDataElementId ?? string.Empty, data.ReportDataCollectorId.ToString() ?? string.Empty);
        }

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
