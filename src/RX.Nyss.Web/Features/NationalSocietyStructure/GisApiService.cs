using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Features.NationalSocietyStructure
{
    public interface IGisApiService
    {
        Task<Result<ReverseLookupResultDto>> ReverseLookup(int nationalSocietyId, double latitude, double longitude);
    }

    public class GisApiService : IGisApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly INyssContext _nyssContext;
        private readonly string _gisApiBaseUrl;
        private readonly string _gisApiKey;

        public GisApiService(IHttpClientFactory httpClientFactory, INyssContext nyssContext, INyssWebConfig config)
        {
            _httpClientFactory = httpClientFactory;
            _nyssContext = nyssContext;

            _gisApiBaseUrl = config.ConnectionStrings.GisApi;
            _gisApiKey = config.GisApiKey;
        }

        public async Task<Result<ReverseLookupResultDto>> ReverseLookup(int nationalSocietyId, double latitude, double longitude)
        {
            var countryCode = await _nyssContext.NationalSocieties.Where(ns => ns.Id == nationalSocietyId).Select(ns => ns.Country.CountryCode).FirstOrDefaultAsync();

            var httpClient = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(_gisApiBaseUrl, UriKind.Absolute), $"/api/reverseGeocode/{countryCode}/{longitude}/{latitude}"));
            request.Headers.Add("x-functions-key", _gisApiKey);

            var result = await httpClient.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                var matches = await JsonSerializer.DeserializeAsync<ReverseLookupResultDto>(await result.Content.ReadAsStreamAsync());
                return Result.Success(matches);
            }

            return Result.Error<ReverseLookupResultDto>("No match");
        }
    }

    public class GisLocationDto
    {
        public string name { get; set; }
        public string admin0Name { get; set; }
        public string admin1Name { get; set; }
        public string admin2Name { get; set; }
        public string admin3Name { get; set; }
        public string admin4Name { get; set; }
        public double distance { get; set; }
        public string type { get; set; }

    }

    public class ReverseLookupResultDto
    {
        public int count { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public List<GisLocationDto> matches { get; set; }

    }
}
