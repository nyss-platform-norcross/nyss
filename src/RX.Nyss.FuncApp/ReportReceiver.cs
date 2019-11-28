using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace RX.Nyss.FuncApp
{
    public class ReportReceiver
    {
        private const string ApiKeyQueryParameterName = "apikey";
        private const int MaxContentLength = 500;
        private readonly ILogger<ReportReceiver> _logger;

        public ReportReceiver(ILogger<ReportReceiver> logger)
        {
            _logger = logger;
        }

        [FunctionName("EnqueueReport")]
        public async Task<IActionResult> EnqueueReport(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "enqueueReport")] HttpRequestMessage httpRequest,
            [ServiceBus("%SERVICEBUS_REPORTQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] IAsyncCollector<string> reportQueue,
            [Blob("%AuthorizedApiKeysBlobPath%", FileAccess.Read)] string authorizedApiKeys)
        {
            if ((httpRequest.Content.Headers.ContentLength ?? int.MaxValue) > MaxContentLength)
            {
                _logger.Log(LogLevel.Warning, $"Received a request with length more than {MaxContentLength} bytes.");
                return new BadRequestResult();
            }

            var httpRequestContent = await httpRequest.Content.ReadAsStringAsync();
            _logger.Log(LogLevel.Debug, $"Received report: {httpRequestContent}.{Environment.NewLine}HTTP request: {httpRequest}");

            if (string.IsNullOrWhiteSpace(httpRequestContent))
            {
                _logger.Log(LogLevel.Warning, "Received an empty report.");
                return new BadRequestResult();
            }

            var decodedHttpRequestContent = HttpUtility.UrlDecode(httpRequestContent);

            if (!VerifyApiKey(authorizedApiKeys, decodedHttpRequestContent))
            {
                return new UnauthorizedResult();
            }

            await reportQueue.AddAsync(httpRequestContent);

            return new OkResult();
        }

        private bool VerifyApiKey(string authorizedApiKeys, string decodedHttpRequestContent)
        {
            if (string.IsNullOrWhiteSpace(authorizedApiKeys))
            {
                _logger.Log(LogLevel.Critical, "The authorized API key list is empty.");
                return false;
            }

            var authorizedApiKeyList = authorizedApiKeys.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var apiKey = HttpUtility.ParseQueryString(decodedHttpRequestContent)[ApiKeyQueryParameterName];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.Log(LogLevel.Warning, "Received a report with an empty API key.");
                return false;
            }
            
            if (!authorizedApiKeyList.Contains(apiKey))
            {
                _logger.Log(LogLevel.Warning, $"Received a report with not authorized API key: {apiKey}.");
                return false;
            }

            return true;
        }
    }
}
