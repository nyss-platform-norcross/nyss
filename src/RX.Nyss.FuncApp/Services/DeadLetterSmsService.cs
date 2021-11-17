using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Extensions;

namespace RX.Nyss.FuncApp.Services
{
    public interface IDeadLetterSmsService
    {
        Task<bool> ResubmitDeadLetterMessages();
    }

    public class DeadLetterSmsService : IDeadLetterSmsService
    {
        private readonly IConfig _config;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ILogger<DeadLetterSmsService> _logger;
        private readonly string _sendSmsQueue;

        public DeadLetterSmsService(IConfig config, IConfiguration configuration, ILogger<DeadLetterSmsService> logger)
        {
            _config = config;
            _serviceBusClient = new ServiceBusClient(configuration["SERVICEBUS_CONNECTIONSTRING"]);
            _logger = logger;
            _sendSmsQueue = configuration["SERVICEBUS_SENDSMSQUEUE"];
        }

        public async Task<bool> ResubmitDeadLetterMessages()
        {
            if (!_config.EnableResendingFeedbackMessages)
            {
                return true;
            }

            _logger.LogInformation("Resubmitting dead lettered feedback sms messages");

            var maxTimeToWaitForMessage = TimeSpan.FromSeconds(30);
            var receiver = _serviceBusClient.CreateReceiver(_sendSmsQueue, new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock,
                SubQueue = SubQueue.DeadLetter
            });
            var sender = _serviceBusClient.CreateSender(_sendSmsQueue);
            var count = 0;

            await foreach(var message in receiver.GetMessages(maxTimeToWaitForMessage))
            {
                try
                {
                    var newMessage = new ServiceBusMessage
                    {
                        Body = message.Body,
                        ContentType = message.ContentType
                    };

                    await sender.SendMessageAsync(newMessage);

                    await receiver.CompleteMessageAsync(message);

                    count++;
                }
                catch (Exception ex)
                {
                    // Releases messages lock
                    await receiver.AbandonMessageAsync(message);

                    _logger.LogError(ex, $"Failed to resend sms message (Id={message.MessageId})");
                }
            }

            _logger.LogInformation($"Resubmitted {count} dead lettered feedback sms messages");

            return true;
        }
    }
}
