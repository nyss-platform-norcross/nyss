using System.Net;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using RX.Nyss.PublicApiFuncApp.Configuration;
using System.Reflection;

namespace RX.Nyss.PublicApiFuncApp;

public class HealthCheck
{
    private readonly ILogger<HealthCheck> _logger;
    private readonly IConfig _config;

    public HealthCheck(ILogger<HealthCheck> logger, IConfig config)
    {
        _logger = logger;
        _config = config;
    }

    [Function("Ping")]
    public HttpResponseData Ping(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequestData httpRequest)
    {
        _logger.Log(LogLevel.Debug, "Received ping request");
        var json = System.Text.Json.JsonSerializer.Serialize(new { Message = "I am alive!" });
        var response = httpRequest.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        var bytes = Encoding.UTF8.GetBytes(json);
        response.Body = new MemoryStream(bytes);
        return response;
    }

    [Function("Version")]
    public HttpResponseData Version(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "version")] HttpRequestData httpRequest)
    {
        var assemblyName = Assembly.GetExecutingAssembly().GetName();
        var version = assemblyName.Version;

        var result = new
        {
            assemblyName.Name,
            Version = version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "unknown",
            ReleaseName = _config.ReleaseName,
            Framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
        };
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        var response = httpRequest.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        var bytes = Encoding.UTF8.GetBytes(json);
        response.Body = new MemoryStream(bytes);
        return response;
    }
}