using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Services.Geolocation
{
    public interface IGeolocationService
    {
        Task<Result<LocationDto>> GetLocationFromCountry(string country);
        Task<Result<LocationDto>> FetchLocationFromCountry(string country);
    }

    public class GeolocationService : IGeolocationService
    {
        private const string CustomUserAgent = "Geo";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssWebConfig _config;
        private readonly IInMemoryCache _inMemoryCache;

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

        public async Task<Result<LocationDto>> FetchLocationFromCountry(string country)
        {
            try
            {
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

            var baseUri = new Uri(_config.ConnectionStrings.Nominatim);
            var requestUri = new Uri(baseUri, new Uri($"/search/?country={country}&format=json", UriKind.Relative));

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
            httpRequestMessage.Headers.Add("User-Agent", CustomUserAgent); // needed for Nominatim to not get 403 code

            var responseMessage = await httpClient.SendAsync(httpRequestMessage);

            await using var responseStream = await responseMessage.Content.ReadAsStreamAsync();

            return await JsonSerializer.DeserializeAsync<List<NominatimGeolocationResponseDto>>(responseStream);
        }
    }
}
