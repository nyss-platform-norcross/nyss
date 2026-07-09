using Azure.Storage.Blobs;
using RX.Nyss.FuncApp.Contracts;
using RX.Nyss.FuncApp.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace RX.Nyss.FuncApp;

public class SendSmsTrigger
{
    private readonly ISmsService _smsService;
    private readonly ILogger<SendSmsTrigger> _logger;
    private readonly BlobServiceClient _blobServiceClient;

    public SendSmsTrigger(
        ISmsService smsService, 
        ILogger<SendSmsTrigger> logger,
        BlobServiceClient blobServiceClient)
    {
        _smsService = smsService;
        _logger = logger;
        _blobServiceClient = blobServiceClient;
    }

    [Function("SendSms")]
    public async Task SendSms(
        [ServiceBusTrigger("%SERVICEBUS_SENDSMSQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] SendSmsMessage message)
    {
        try
        {
            // Read whitelisted phone numbers
            var whitelistedPhoneNumbersPath = Environment.GetEnvironmentVariable("WhitelistedPhoneNumbersBlobPath");
            if (string.IsNullOrWhiteSpace(whitelistedPhoneNumbersPath))
            {
                _logger.LogError("Environment variable 'WhitelistedPhoneNumbersBlobPath' is not set or empty.");
                throw new InvalidOperationException("Missing WhitelistedPhoneNumbersBlobPath environment variable.");
            }
            var phoneNumbersBlob = GetBlobClient(_blobServiceClient, whitelistedPhoneNumbersPath);
            var phoneNumbersResult = await phoneNumbersBlob.DownloadContentAsync();
            var whitelistedPhoneNumbers = phoneNumbersResult.Value.Content.ToString();

            await _smsService.SendSms(message, whitelistedPhoneNumbers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SendSms trigger");
            throw;
        }
    }

    private BlobClient GetBlobClient(BlobServiceClient serviceClient, string blobPath)
    {
        var parts = blobPath.Split('/', 2);
        var containerName = parts[0];
        var blobName = parts.Length > 1 ? parts[1] : string.Empty;
        
        return serviceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
    }
}