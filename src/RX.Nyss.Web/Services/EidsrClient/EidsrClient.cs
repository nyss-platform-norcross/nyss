using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Services.EidsrClient.Dto;
using RX.Nyss.Web.Utils;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Services.EidsrClient;

public interface IEidsrClient
{
    Task<Result<EidsrOrganisationUnitsResponse>> GetOrganizationUnits(EidsrApiProperties apiProperties, string programId);

    Task<Result<EidsrProgramResponse>> GetProgramFromApi(EidsrApiProperties apiProperties, string programId);

}

public class EidsrClient : IEidsrClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerAdapter _loggerAdapter;
    private readonly INyssWebConfig _config;
    private readonly IInMemoryCache _inMemoryCache;

    public EidsrClient(
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

    public async Task<Result<EidsrOrganisationUnitsResponse>> GetOrganizationUnits(EidsrApiProperties apiProperties, string programId)
    {
        _inMemoryCache.Remove($"Eidsr_OrganizationUnits_{programId}");
        return await _inMemoryCache.GetCachedResult(
            key: $"Eidsr_OrganizationUnits_{programId}",
            validFor: TimeSpan.FromSeconds(2),
            value: () => GetOrganizationUnitsFromApi(apiProperties, programId));
    }

    public async Task<Result<EidsrProgramResponse>> GetProgramFromApi(
        EidsrApiProperties apiProperties,
        string programId
    )
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var configureResult = ConfigureClient(apiProperties, httpClient);

            if (!configureResult)
            {
                return Error<EidsrProgramResponse>(ResultKey.EidsrIntegration.EidsrApi.ConnectionError);
            }

            var req = new HttpRequestMessage(HttpMethod.Get, $"api/programs/{programId}?" +
                $"fields=id,name");

            var res = await httpClient.SendAsync(req);

            if (res.IsSuccessStatusCode)
            {
                await using var responseStream = await res.Content.ReadAsStreamAsync();
                var response = await JsonSerializer.DeserializeAsync<EidsrProgramResponse>(responseStream);
                return Success(response);
            }
        }
        catch { }

        return Error<EidsrProgramResponse>(ResultKey.EidsrIntegration.EidsrApi.ConnectionError);
    }

    private async Task<Result<EidsrOrganisationUnitsResponse>> GetOrganizationUnitsFromApi(
        EidsrApiProperties apiProperties,
        string programId
        )
    {
        using var httpClient = _httpClientFactory.CreateClient();
        ConfigureClient(apiProperties, httpClient);

        var req = new HttpRequestMessage(HttpMethod.Get, $"api/organisationUnits?" +
            $"filter=programs.id:eq:{programId}&" +
            $"fields=id,displayName&" +
            $"paging=false");

        var res = await httpClient.SendAsync(req);

        if (res.IsSuccessStatusCode)
        {
            await using var responseStream = await res.Content.ReadAsStreamAsync();
            var response = await JsonSerializer.DeserializeAsync<EidsrOrganisationUnitsResponse>(responseStream);
            return Success(response);
        }

        _loggerAdapter.Error(res.ReasonPhrase);
        return Error<EidsrOrganisationUnitsResponse>(ResultKey.EidsrIntegration.EidsrApi.ConnectionError);
    }

    private static bool ConfigureClient(EidsrApiProperties apiProperties, HttpClient httpClient)
    {
        Uri.TryCreate(apiProperties.Url, UriKind.Absolute, out var validatedUri);

        if (validatedUri == default)
        {
            return false;
        }

        httpClient.BaseAddress = validatedUri;

        httpClient.DefaultRequestHeaders.Authorization =
            new BasicAuthenticationHeaderValue($"{apiProperties.UserName}:{apiProperties.Password}");

        return true;
    }
}