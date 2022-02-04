using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
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
            [Blob("%PublicStatsBlobObjectPath%", FileAccess.ReadWrite, Connection = "DATABLOBSTORAGE_CONNECTIONSTRING")] BlobClient statsBlob)
        {
            _logger.LogInformation("Getting public stats from data blob");
            BlobDownloadResult blobContent = await statsBlob.DownloadContentAsync();
            var stats = blobContent.Content.ToString();

            if (string.IsNullOrEmpty(stats))
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(JsonConvert.DeserializeObject(stats));
        }
    }
}
