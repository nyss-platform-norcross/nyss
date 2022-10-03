using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RX.Nyss.Web.Services.EidsrClient.Dto;

public class EidsrOrganisationUnitsResponse
{
    [JsonPropertyName("organisationUnits")]
    public List<EidsrOrganizationUnit> OrganisationUnits { get; set; }
}
