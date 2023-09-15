using System;
using System.Net.Http;
using RX.Nyss.Common.Services.SmsGatewayClient.Dto;
using RX.Nyss.Common.Utils.Logging;

namespace RX.Nyss.Common.Services.SmsGatewayClient;

public interface ISmsClient
{
    
}

public class SmsClient : ISmsClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerAdapter _loggerAdapter;

    public SmsClient(
        IHttpClientFactory httpClientFactory,
        ILoggerAdapter loggerAdapter)
    {
        _httpClientFactory = httpClientFactory;
        _loggerAdapter = loggerAdapter;
    }

    


    private static bool ConfigureClient(SmsApiProperties apiProperties, HttpClient httpClient)
    {
        Uri.TryCreate(apiProperties.Url, UriKind.Absolute, out var validatedUri);

        if (validatedUri == default)
        {
            return false;
        }

        httpClient.BaseAddress = validatedUri;

        httpClient.DefaultRequestHeaders.Authorization =
            new SmsGatewayAuthenticationHeaderValue($"{apiProperties.UserName}:{apiProperties.Password}");

        return true;
    }
}