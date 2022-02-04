using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RX.Nyss.FuncApp.Configuration;

namespace RX.Nyss.FuncApp.Services;

public interface IDeadLetterSmsService
{
    Task<bool> ResubmitDeadLetterMessages();
}

public class DeadLetterSmsService : IDeadLetterSmsService
{
    private readonly IConfig _config;
    private readonly ILogger<DeadLetterSmsService> _logger;
    private readonly ServiceBusReceiver _receiver;
    private readonly ServiceBusSender _sender;

    public DeadLetterSmsService(IConfig config, IConfiguration configuration, ILogger<DeadLetterSmsService> logger, ServiceBusClient serviceBusClient)
    {
        _config = config;
        _logger = logger;
        _receiver = serviceBusClient.CreateReceiver(configuration["SERVICEBUS_SENDSMSQUEUE"], new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock,
            SubQueue = SubQueue.DeadLetter
        });
        _sender = serviceBusClient.CreateSender(configuration["SERVICEBUS_SENDSMSQUEUE"]);
    }

    public async Task<bool> ResubmitDeadLetterMessages()
    {
        if (!_config.EnableResendingFeedbackMessages)
        {
            return true;
        }

        _logger.LogInformation("Resubmitting dead lettered feedback sms messages");

        var maxTimeToWaitForMessage = TimeSpan.FromSeconds(30);
        var count = 0;

        var messages = await _receiver.ReceiveMessagesAsync(_config.NumberOfMessagesToFetchForResending, maxTimeToWaitForMessage);

        foreach(var message in messages)
        {
            try
            {
                var newMessage = new ServiceBusMessage
                {
                    Body = message.Body,
                    ContentType = message.ContentType
                };

                await _sender.SendMessageAsync(newMessage);

                await _receiver.CompleteMessageAsync(message);

                count++;
            }
            catch (Exception ex)
            {
                // Releases messages lock
                await _receiver.AbandonMessageAsync(message);

                _logger.LogError(ex, $"Failed to resend sms message (Id={message.MessageId})");
            }
        }

        _logger.LogInformation($"Resubmitted {count} dead lettered feedback sms messages");

        return true;
    }
}