using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Services
{
    public interface IQueueService
    {
        Task Send<T>(string queueName, T data);
    }

    public class QueueService : IQueueService
    {
        private readonly INyssWebConfig _config;
        private readonly ServiceBusClient _serviceBusClient;

        public QueueService(INyssWebConfig config, ServiceBusClient serviceBusClient)
        {
            _config = config;
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
    }
}
