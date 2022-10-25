using System.Collections.Generic;

namespace RX.Nyss.Common.Services.EidsrClient.Dto;

public class EidsrRegisterEventRequestBody
{
    public string Program { get; set; }
    public string OrgUnit { get; set; }
    public string EventDate { get; set; }
    public List<DataValue> DataValues { get; set; }

    public class DataValue
    {
        public string DataElement { get; set; }
        public string Value { get; set; }
    }
}
