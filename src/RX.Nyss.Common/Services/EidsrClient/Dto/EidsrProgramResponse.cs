using System.Text.Json.Serialization;

namespace RX.Nyss.Web.Services.EidsrClient.Dto;

public class EidsrProgramResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
