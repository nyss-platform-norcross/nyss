using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Contracts;

namespace RX.Nyss.FuncApp.Services
{
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
            var isWhitelisted = _config.MailjetConfig.SendFeedbackSmsToAll || _whitelistValidator.IsWhiteListedPhoneNumber(message.PhoneNumber, whitelistedPhoneNumbers);

            if (isWhitelisted)
            {
                var cloudToDeviceMethod = new CloudToDeviceMethod("send_sms", TimeSpan.FromSeconds(30));
                cloudToDeviceMethod.SetPayloadJson(JsonSerializer.Serialize(new
                {
                    To = message.PhoneNumber,
                    Message = message.SmsMessage
                }));

                await _iotHubServiceClient.InvokeDeviceMethodAsync(message.IotHubDeviceName, cloudToDeviceMethod);
            }
        }
    }
}
