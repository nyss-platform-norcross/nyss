using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace RX.Nyss.PublicApiFuncApp
{
    public class PublicStatsTrigger
    {
        private readonly ILogger<PublicStatsTrigger> _logger;

        public PublicStatsTrigger(ILogger<PublicStatsTrigger> logger)
        {
            _logger = logger;
        }

        [FunctionName("Stats")]
        public async Task<IActionResult> Stats(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stats")] HttpRequestMessage httpRequestMessage,
            [Blob("%PublicStatsBlobObjectPath%", FileAccess.Read)] CloudBlockBlob statsBlob)
        {
            _logger.LogInformation("Downloading public stats from blob");
            var stats = await statsBlob.DownloadTextAsync();
            return new OkObjectResult(JsonConvert.DeserializeObject(stats));
        }
    }
}
