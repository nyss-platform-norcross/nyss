using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RX.Nyss.Common.Services.DhisClient.Dto;
public class DhisRegisterReportRequestBody
{
    [JsonPropertyName("program")]
    public string? Program { get; set; }

    [JsonPropertyName("orgUnit")]
    public string OrgUnit { get; set; } = string.Empty;

    [JsonPropertyName("eventDate")]
    public string EventDate { get; set; } = string.Empty;

    [JsonPropertyName("dataValues")]
    public List<DataValue> DataValues { get; set; } = new List<DataValue>();

    public class DataValue
    {
        [JsonPropertyName("dataElement")]
        public string DataElement { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
}
