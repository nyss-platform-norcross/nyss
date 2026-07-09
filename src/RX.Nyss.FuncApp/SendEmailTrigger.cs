using Azure.Storage.Blobs;
using RX.Nyss.FuncApp.Contracts;
using RX.Nyss.FuncApp.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace RX.Nyss.FuncApp;

public class SendEmailTrigger
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailTrigger> _logger;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobServiceClient _generalBlobServiceClient;

    public SendEmailTrigger(
        IEmailService emailService, 
        ILogger<SendEmailTrigger> logger,
        BlobServiceClient blobServiceClient)
    {
        _emailService = emailService;
        _logger = logger;
        _blobServiceClient = blobServiceClient;
        
        // For the general blob storage with different connection string
        // This will be injected separately - see Program.cs updates below
        _generalBlobServiceClient = blobServiceClient;
    }

    [Function("SendEmail")]
    public async Task SendEmail(
        [ServiceBusTrigger("%SERVICEBUS_SENDEMAILQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] SendEmailMessage message)
    {
        try
        {
            // Read whitelisted phone numbers
            var whitelistedPhoneNumbersPath = Environment.GetEnvironmentVariable("WhitelistedPhoneNumbersBlobPath");
            if (string.IsNullOrEmpty(whitelistedPhoneNumbersPath))
                throw new InvalidOperationException("WhitelistedPhoneNumbersBlobPath environment variable is not set");
            var phoneNumbersBlob = GetBlobClient(_blobServiceClient, whitelistedPhoneNumbersPath);
            var phoneNumbersResult = await phoneNumbersBlob.DownloadContentAsync();
            var whitelistedPhoneNumbers = phoneNumbersResult.Value.Content.ToString();

            // Read whitelisted email addresses
            var whitelistedEmailAddressesPath = Environment.GetEnvironmentVariable("WhitelistedEmailAddressesBlobPath");
            if (string.IsNullOrEmpty(whitelistedEmailAddressesPath))
                throw new InvalidOperationException("WhitelistedEmailAddressesBlobPath environment variable is not set");
            var emailAddressesBlob = GetBlobClient(_blobServiceClient, whitelistedEmailAddressesPath);
            var emailAddressesResult = await emailAddressesBlob.DownloadContentAsync();
            var whitelistedEmailAddresses = emailAddressesResult.Value.Content.ToString();

            // Get general blob container
            var generalBlobContainerName = Environment.GetEnvironmentVariable("GeneralBlobContainerName");
            if (string.IsNullOrEmpty(generalBlobContainerName))
                throw new InvalidOperationException("GeneralBlobContainerName environment variable is not set");
            var blobContainerClient = _generalBlobServiceClient.GetBlobContainerClient(generalBlobContainerName);

            await _emailService.SendEmail(message, whitelistedEmailAddresses, whitelistedPhoneNumbers, blobContainerClient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SendEmail trigger");
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