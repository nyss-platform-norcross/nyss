using System;
using System.Collections.Generic;
using Azure.Messaging.ServiceBus;

namespace RX.Nyss.FuncApp.Extensions
{
    public static class ServiceBusReceiverExtensions
    {
        public static async IAsyncEnumerable<ServiceBusReceivedMessage> GetMessages(this ServiceBusReceiver receiver, TimeSpan maxWaitTime)
        {
            while (true)
            {
                var message = await receiver.ReceiveMessageAsync(maxWaitTime);

                if (message == null)
                {
                    yield break;
                }

                yield return message;
            }
        }
    }
}
