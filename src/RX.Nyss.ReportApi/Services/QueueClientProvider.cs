using Microsoft.Azure.ServiceBus;
using RX.Nyss.ReportApi.Configuration;

namespace RX.Nyss.ReportApi.Services
{
    public interface IQueueClientProvider
    {
        IQueueClient GetClient(string queueName);
    }

    public class QueueClientProvider : IQueueClientProvider
    {
        private readonly INyssReportApiConfig _config;

        public QueueClientProvider(INyssReportApiConfig config)
        {
            _config = config;
        }

        public IQueueClient GetClient(string queueName) => new QueueClient(_config.ConnectionStrings.ServiceBus, queueName);
    }
}
