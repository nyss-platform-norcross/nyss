using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Utils;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Services.Geolocation
{
    public interface IGeolocationService
    {
        Task<Result<LocationDto>> GetLocationFromCountry(string country);
    }

    public class GeolocationService : IGeolocationService
    {
        private const string _customUserAgent = "Geo";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssWebConfig _config;
        private readonly IInMemoryCache _inMemoryCache;

        private readonly IReadOnlyDictionary<string, LocationDto> _knownFakeCountries = new Dictionary<string, LocationDto>
        {
            {
                "Mandawi", new LocationDto
                {
                    Longitude = 10.744628906250002, // Oslo, Norway
                    Latitude = 59.90822188626548
                }
            }
        };

        public GeolocationService(
            IHttpClientFactory httpClientFactory,
            ILoggerAdapter loggerAdapter,
            INyssWebConfig config,
            IInMemoryCache inMemoryCache)
        {
            _httpClientFactory = httpClientFactory;
            _loggerAdapter = loggerAdapter;
            _config = config;
            _inMemoryCache = inMemoryCache;
        }

        public async Task<Result<LocationDto>> GetLocationFromCountry(string country) =>
            await _inMemoryCache.GetCachedResult(
                key: $"Geolocation_Country_{country}",
                validFor: TimeSpan.FromDays(1),
                value: () => FetchLocationFromCountry(country));

        private async Task<Result<LocationDto>> FetchLocationFromCountry(string country)
        {
            try
            {
                if (_knownFakeCountries.ContainsKey(country))
                {
                    return Success(_knownFakeCountries[country]);
                }

                var value = await GetGeolocationResponse(country);

                if (value == null || value.Count == 0)
                {
                    return Error<LocationDto>(ResultKey.Geolocation.NotFound);
                }

                var location = value.Select(g => new LocationDto
                {
                    Longitude = double.Parse(g.Longitude, CultureInfo.InvariantCulture),
                    Latitude = double.Parse(g.Latitude, CultureInfo.InvariantCulture)
                })
                    .First();

                return Success(location);
            }
            catch (Exception exception)
            {
                _loggerAdapter.Error(exception, "There was a problem with retrieving data from Nominatim API");
                return Error<LocationDto>(ResultKey.UnexpectedError);
            }
        }

        private async Task<List<NominatimGeolocationResponseDto>> GetGeolocationResponse(string country)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            var baseUri = new Uri(_config.ConnectionStrings.Nominatim.TrimEnd('/') + '/');
            var encodedCountry = Uri.EscapeDataString(country);
            var requestUri = new Uri(baseUri, $"search?country={encodedCountry}&format=json");

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
            httpRequestMessage.Headers.Add("User-Agent", _customUserAgent);

            var responseMessage = await httpClient.SendAsync(httpRequestMessage);

            if (!responseMessage.IsSuccessStatusCode)
            {
                var errorContent = await responseMessage.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Nominatim API request failed with status {responseMessage.StatusCode}. Response: {errorContent}");
            }

            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            _loggerAdapter.Info(responseContent);

            if (string.IsNullOrWhiteSpace(responseContent) || responseContent == "[]")
            {
                return new List<NominatimGeolocationResponseDto>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<NominatimGeolocationResponseDto>>(responseContent)
                    ?? new List<NominatimGeolocationResponseDto>();
            }
            catch (JsonException ex)
            {
                throw new JsonException(
                    $"Failed to deserialize Nominatim response. Content: {responseContent}", ex);
            }
        }
    }
}
