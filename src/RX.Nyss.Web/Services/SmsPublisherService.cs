using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Common.Services;
using Telerivet.Client;
using RX.Nyss.Common.Utils.Logging;

namespace RX.Nyss.Web.Services;

public interface ISmsPublisherService
{
    Task SendSms(string iotHubDeviceName, List<SendSmsRecipient> recipients, string smsMessage);
}

public class SmsPublisherService : ISmsPublisherService
{
    private readonly ServiceBusSender _sender;
    private readonly ICryptographyService _cryptographyService;
    private readonly INyssWebConfig _config;
    private readonly ILoggerAdapter _loggerAdapter;

    public SmsPublisherService(INyssWebConfig config, ServiceBusClient serviceBusClient, ICryptographyService cryptographyService, ILoggerAdapter loggerAdapter)
    {
        _sender = serviceBusClient.CreateSender(config.ServiceBusQueues.SendSmsQueue);
        _cryptographyService = cryptographyService;
        _config = config;
        _loggerAdapter = loggerAdapter;
    }

    public async Task SendSms(string iotHubDeviceName, List<SendSmsRecipient> recipients, string smsMessage) =>
        await Task.WhenAll(recipients.Select(recipient =>
        {
            var sendSms = new SendSmsMessage
            {
                IotHubDeviceName = iotHubDeviceName,
                PhoneNumber = recipient.PhoneNumber,
                SmsMessage = smsMessage,
                ModemNumber = recipient.Modem
            };

            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sendSms)))
            {
                Subject = "RX.Nyss.Web",
                ApplicationProperties = { { "IoTHubDevice", iotHubDeviceName } }
            };

            return _sender.SendMessageAsync(message);
        }));

    public async Task SendTelerivetSms(long number, string message, string apiKey, string projectId) =>
        await Task.Run(async () =>
    {
        ///////////////////////////////////////////////////////
        var telerivetApiKey = _cryptographyService.Decrypt(
            apiKey,
            _config.VariableKey,
            _config.SupplementaryKey);
        ////////////////////////////////////////////////////////
        var tr = new TelerivetAPI(telerivetApiKey);
        var project = tr.InitProjectById(projectId);
        // send message
        var sentMsg = await project.SendMessageAsync(Util.Options("content", message, "to_number", number));
        _loggerAdapter.Info(sentMsg);
    });
}

public class SendSmsMessage
{
    public string IotHubDeviceName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string SmsMessage { get; set; } = string.Empty;
    public int? ModemNumber { get; set; }
}

public class SendSmsRecipient
{
    public string PhoneNumber { get; set; } = string.Empty;
    public int? Modem { get; set; }
}
