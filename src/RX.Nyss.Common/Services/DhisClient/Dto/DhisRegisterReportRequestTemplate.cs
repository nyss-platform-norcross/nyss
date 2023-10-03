using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RX.Nyss.Web.Services.EidsrClient.Dto;

namespace RX.Nyss.Common.Services.DhisClient.Dto;
public class DhisRegisterReportRequestTemplate
{
    public string Program { get; set; }

    public EidsrApiProperties EidsrApiProperties { get; set; }

    public string LocationDataElementId { get; set; }

    public string DateOfOnsetDataElementId { get; set; }

    public string PhoneNumberDataElementId { get; set; }

    public string SuspectedDiseaseDataElementId { get; set; }

    public string EventTypeDataElementId { get; set; }

    public string GenderDataElementId { get; set; }
}
