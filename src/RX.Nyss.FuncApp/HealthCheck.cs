using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using RX.Nyss.FuncApp.Configuration;

namespace RX.Nyss.FuncApp;

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
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequestData request)
    {
        _logger.LogDebug("Received ping request");
        
        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        response.WriteString("I am alive!");
        
        return response;
    }

    [Function("Version")]
    public async Task<HttpResponseData> Version(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "version")] HttpRequestData request)
    {
        var assemblyName = Assembly.GetExecutingAssembly().GetName();
        var version = assemblyName.Version;

        var versionInfo = new
        {
            assemblyName.Name,
            Version = $"{version?.Major}.{version?.Minor}.{version?.Build}",
            ReleaseName = _config.ReleaseName,
            Framework = RuntimeInformation.FrameworkDescription
        };

        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(versionInfo);
        
        return response;
    }
}