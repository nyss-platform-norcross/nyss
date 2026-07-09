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

            var req = new HttpRequestMessage(HttpMethod.Get, $"api/programs/{programId}?fields=id,name");
            var res = await httpClient.SendAsync(req);

            if (res.IsSuccessStatusCode)
            {
                await using var responseStream = await res.Content.ReadAsStreamAsync();
                var response = await JsonSerializer.DeserializeAsync<EidsrProgramResponse>(responseStream);

                if (response == null)
                {
                    _loggerAdapter.Error("GetProgram: deserialized response was null.");
                    return Error<EidsrProgramResponse>(ResultKey.EidsrIntegration.EidsrApi.ConnectionError);
                }

                return Success(response);
            }

            _loggerAdapter.Error($"GetProgram: request failed with status {res.StatusCode}.");
        }
        catch (Exception ex)
        {
            _loggerAdapter.Error(ex, "GetProgram: unexpected error.");
        }

        return Error<EidsrProgramResponse>(ResultKey.EidsrIntegration.EidsrApi.ConnectionError);
    }

    public async Task<Result<EidsrOrganisationUnitsResponse>> GetOrganizationUnits(
        EidsrApiProperties apiProperties,
        string programId)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var configureResult = ConfigureClient(apiProperties, httpClient);

            if (!configureResult)
            {
                return Error<EidsrOrganisationUnitsResponse>(ResultKey.EidsrIntegration.EidsrApi.ConnectionError);
            }

            var req = new HttpRequestMessage(HttpMethod.Get, $"api/organisationUnits?" +
                $"filter=programs.id:eq:{programId}&" +
                $"fields=id,displayName&" +
                $"paging=false");

            var res = await httpClient.SendAsync(req);

            if (res.IsSuccessStatusCode)
            {
                await using var responseStream = await res.Content.ReadAsStreamAsync();
                var response = await JsonSerializer.DeserializeAsync<EidsrOrganisationUnitsResponse>(responseStream);

                if (response == null)
                {
                    _loggerAdapter.Error("GetOrganizationUnits: deserialized response was null.");
                    return Error<EidsrOrganisationUnitsResponse>(ResultKey.EidsrIntegration.EidsrApi.ConnectionError);
                }

                return Success(response);
            }

            _loggerAdapter.Error($"GetOrganizationUnits: request failed with status {res.StatusCode} - {res.ReasonPhrase}.");
        }
        catch (Exception ex)
        {
            _loggerAdapter.Error(ex, "GetOrganizationUnits: unexpected error.");
        }

        return Error<EidsrOrganisationUnitsResponse>(ResultKey.EidsrIntegration.EidsrApi.ConnectionError);
    }

    public async Task<Result> RegisterEvent(
        EidsrRegisterEventRequest eidsrRegisterEventRequest)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var configureResult = ConfigureClient(eidsrRegisterEventRequest.EidsrApiProperties, httpClient);

            if (!configureResult)
            {
                return Error(ResultKey.EidsrIntegration.EidsrApi.RegisterEventError);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "api/events")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(eidsrRegisterEventRequest.EidsrRegisterEventRequestBody),
                    Encoding.UTF8,
                    "application/json")
            };

            var res = await httpClient.SendAsync(request);

            if (res.IsSuccessStatusCode)
            {
                return Success();
            }

            _loggerAdapter.Error($"RegisterEvent: request failed with status {res.StatusCode} - {res.ReasonPhrase}.");
        }
        catch (Exception ex)
        {
            _loggerAdapter.Error(ex, "RegisterEvent: unexpected error.");
        }

        return Error(ResultKey.EidsrIntegration.EidsrApi.RegisterEventError);
    }

    private static bool ConfigureClient(EidsrApiProperties apiProperties, HttpClient httpClient)
    {
        if (!Uri.TryCreate(apiProperties.Url, UriKind.Absolute, out var validatedUri))
        {
            return false;
        }

        httpClient.BaseAddress = validatedUri;
        httpClient.DefaultRequestHeaders.Authorization =
            new BasicAuthenticationHeaderValue($"{apiProperties.UserName}:{apiProperties.Password}");

        return true;
    }
}