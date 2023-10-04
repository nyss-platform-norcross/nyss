using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RX.Nyss.Common.Services.DhisClient.Dto;
using RX.Nyss.Common.Services.EidsrClient.Dto;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Web.Services.EidsrClient.Dto;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Common.Services.DhisClient;

public interface IDhisClient
{
    Task<Result> RegisterReport(DhisRegisterReportRequest dhisRegisterReportRequest);
}
public class DhisClient : IDhisClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerAdapter _loggerAdapter;

    public DhisClient(
        IHttpClientFactory httpClientFactory,
        ILoggerAdapter loggerAdapter)
    {
        _httpClientFactory = httpClientFactory;
        _loggerAdapter = loggerAdapter;
    }

    public async Task<Result> RegisterReport(
        DhisRegisterReportRequest dhisRegisterReportRequest)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        ConfigureClient(dhisRegisterReportRequest.EidsrApiProperties, httpClient);

        var request = new HttpRequestMessage(HttpMethod.Post, "api/events");
        request.Content = new StringContent(
            JsonSerializer.Serialize(dhisRegisterReportRequest.DhisRegisterReportRequestBody), Encoding.UTF8, "application/json");

        var res = await httpClient.SendAsync(request);

        if (res.IsSuccessStatusCode)
        {
            return Success();
        }

        _loggerAdapter.Error(res.ReasonPhrase);
        return Error<bool>(ResultKey.DhisIntegration.DhisApi.RegisterReportError);
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