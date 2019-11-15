using System.Text.Json.Serialization;

namespace RX.Nyss.Web.Services.Geolocation
{
    public class NominatimGeolocationResponseDto
    {
        [JsonPropertyName("lat")]
        public string Latitude { get; set; }

        [JsonPropertyName("lon")]
        public string Longitude { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
