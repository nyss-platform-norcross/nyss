using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RX.Nyss.Common.Utils;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Contracts;

namespace RX.Nyss.FuncApp.Services;

public interface ISmsService
{
    Task SendSms(SendSmsMessage message, string whitelistedPhoneNumbers);
}

public class SmsService : ISmsService
{
    private readonly IConfig _config;
    private readonly ILogger<SmsService> _logger;
    private readonly IWhitelistValidator _whitelistValidator;
    private readonly ServiceClient _iotHubServiceClient;

    public SmsService(ILogger<SmsService> logger, IConfig config, IWhitelistValidator whitelistValidator)
    {
        _logger = logger;
        _config = config;
        _whitelistValidator = whitelistValidator;
        _iotHubServiceClient = ServiceClient.CreateFromConnectionString(config.ConnectionStrings.IotHubService);
    }

    public async Task SendSms(SendSmsMessage message, string whitelistedPhoneNumbers)
    {
        _logger.LogDebug($"Sending sms to phone number ending with '{message.PhoneNumber.SubstringFromEnd(4)}...' through IOT device {message.IotHubDeviceName}...");

        var cloudToDeviceMethod = new CloudToDeviceMethod("send_sms", TimeSpan.FromSeconds(60));
        cloudToDeviceMethod.SetPayloadJson(JsonConvert.SerializeObject(new SmsIoTHubMessage
        {
            To = message.PhoneNumber,
            Message = message.SmsMessage,
            ModemNumber = message.ModemNumber,
            Unicode = "1"
        }, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

        var response = await _iotHubServiceClient.InvokeDeviceMethodAsync(message.IotHubDeviceName, cloudToDeviceMethod);

        if (response.Status != 200)
        {
            throw new Exception($"Failed to send sms to device {message.IotHubDeviceName}, {response.GetPayloadAsJson()}");
        }
    }
}