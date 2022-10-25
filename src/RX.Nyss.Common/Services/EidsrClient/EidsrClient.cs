using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RX.Nyss.Common.Services.EidsrClient.Dto;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Web.Services.EidsrClient.Dto;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Common.Services.EidsrClient;

public interface IEidsrClient
{
    Task<Result<EidsrOrganisationUnitsResponse>> GetOrganizationUnits(EidsrApiProperties apiProperties, string programId);

    Task<Result<EidsrProgramResponse>> GetProgram(EidsrApiProperties apiProperties, string programId);

    Task<Result> RegisterEvent(EidsrRegisterEventRequest eidsrRegisterEventRequest);
}

public class EidsrClient : IEidsrClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerAdapter _loggerAdapter;

    public EidsrClient(
        IHttpClientFactory httpClientFactory,
        ILoggerAdapter loggerAdapter)
    {
        _httpClientFactory = httpClientFactory;
        _loggerAdapter = loggerAdapter;
    }

    public async Task<Result<EidsrProgramResponse>> GetProgram(
        EidsrApiProperties apiProperties,
        string programId)
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

    public async Task<Result<EidsrOrganisationUnitsResponse>> GetOrganizationUnits(
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

    public async Task<Result> RegisterEvent(
        EidsrRegisterEventRequest eidsrRegisterEventRequest)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        ConfigureClient(eidsrRegisterEventRequest.EidsrApiProperties, httpClient);

        var request = new HttpRequestMessage(HttpMethod.Post, "api/events");
        request.Content = new StringContent(
            JsonSerializer.Serialize(eidsrRegisterEventRequest.EidsrRegisterEventRequestBody), Encoding.UTF8, "application/json");

        var res = await httpClient.SendAsync(request);

        if (res.IsSuccessStatusCode)
        {
            return Success();
        }

        _loggerAdapter.Error(res.ReasonPhrase);
        return Error<bool>(ResultKey.EidsrIntegration.EidsrApi.RegisterEventError);
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