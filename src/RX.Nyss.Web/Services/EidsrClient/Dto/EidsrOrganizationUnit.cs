using System.Text.Json.Serialization;

namespace RX.Nyss.Web.Services.EidsrClient.Dto;

public class EidsrOrganizationUnit
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }
}
