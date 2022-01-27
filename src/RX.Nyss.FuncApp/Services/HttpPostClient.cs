using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RX.Nyss.FuncApp.Services;

public interface IHttpPostClient
{
    Task<HttpResponseMessage> PostJsonAsync<T>(Uri requestUri, T body, IEnumerable<(string key, string value)> headers = null);
}

public class HttpPostClient : IHttpPostClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpPostClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HttpResponseMessage> PostJsonAsync<T>(Uri requestUri, T body, IEnumerable<(string key, string value)> headers = null)
    {
        var payload = JsonSerializer.Serialize(body, options: new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var httpClient = _httpClientFactory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = new StringContent(payload, Encoding.UTF8, "application/json") };
        if (headers != null)
        {
            foreach (var (key, value) in headers)
            {
                request.Headers.Add(key, value);
            }
        }

        var httpResponseMessage = await httpClient.SendAsync(request);

        httpResponseMessage.EnsureSuccessStatusCode();

        return httpResponseMessage;
    }
}