using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace RX.Nyss.Web.Services
{
    public interface IQueueService
    {
        Task Send<T>(string queueName, T data);

        Task SendCollection<T>(string queueName, List<T> data);

    }

    public class QueueService : IQueueService
    {
        private readonly ServiceBusClient _serviceBusClient;

        public QueueService(ServiceBusClient serviceBusClient)
        {
            _serviceBusClient = serviceBusClient;
        }

        public async Task Send<T>(string queueName, T data)
        {
            var sender = _serviceBusClient.CreateSender(queueName);

            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data)))
            {
                Subject = "RX.Nyss.Web"
            };

            await sender.SendMessageAsync(message);
        }

        public async Task SendCollection<T>(string queueName, List<T> collection)
        {
            var sender = _serviceBusClient.CreateSender(queueName);

            await Task.WhenAll(collection.Select(collectionItem =>
            {
                var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(collectionItem)))
                {
                    Subject = "RX.Nyss.Web",
                };

                return sender.SendMessageAsync(message);
            }));
        }
    }
}
