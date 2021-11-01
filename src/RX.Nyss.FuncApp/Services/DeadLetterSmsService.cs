using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using RX.Nyss.FuncApp.Configuration;

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

        public DeadLetterSmsService(IConfig config, ILogger<DeadLetterSmsService> logger)
        {
            _config = config;
            _serviceBusClient = new ServiceBusClient(config.ConnectionStrings.ServiceBus);
            _logger = logger;
        }

        public async Task<bool> ResubmitDeadLetterMessages()
        {
            if (!_config.EnableResendingFeedbackMessages)
            {
                return true;
            }

            _logger.LogInformation("Resubmitting dead lettered feedback sms messages");

            var receiver = _serviceBusClient.CreateReceiver(_config.ServiceBusQueues.SendSmsQueue, new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock,
                SubQueue = SubQueue.DeadLetter
            });
            var sender = _serviceBusClient.CreateSender(_config.ServiceBusQueues.SendSmsQueue);
            var messageBatch = await sender.CreateMessageBatchAsync();
            var deadLetterMessages = await receiver.ReceiveMessagesAsync(_config.MaxMessagesToResubmit);
            var messagesToDeleteFromDeadLetter = new List<ServiceBusReceivedMessage>();

            foreach (var message in deadLetterMessages)
            {
                var newMessage = new ServiceBusMessage
                {
                    Body = message.Body,
                    ContentType = message.ContentType
                };

                if (messageBatch.TryAddMessage(newMessage))
                {
                    messagesToDeleteFromDeadLetter.Add(message);
                }
                else
                {
                    _logger.LogInformation("Resubmitting {MessageBatchCount} feedback sms messages", messageBatch.Count);

                    await sender.SendMessagesAsync(messageBatch);
                    messageBatch.Dispose();
                    messageBatch = await sender.CreateMessageBatchAsync();
                }
            }

            _logger.LogInformation("Resubmitting {MessageBatchCount} feedback sms messages", messageBatch.Count);
            await sender.SendMessagesAsync(messageBatch);

            _logger.LogInformation("Removing {Messages} messages from the dead letter queue", messagesToDeleteFromDeadLetter.Count);
            foreach (var message in messagesToDeleteFromDeadLetter)
            {
                await receiver.CompleteMessageAsync(message);
            }

            return true;
        }
    }
}
