using System.Collections.Generic;
using System.Text.Json.Serialization;
using RX.Nyss.Web.Services.EidsrClient.Dto;

namespace RX.Nyss.Web.Services.EidsrClient;

public class EidsrOrganisationUnitsResponse
{
    [JsonPropertyName("organisationUnits")]
    public List<EidsrOrganizationUnit> OrganisationUnits { get; set; }
}
