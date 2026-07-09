using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RX.Nyss.FuncApp.Contracts;

namespace RX.Nyss.FuncApp.Services;

public interface IMTNReportPublisherService
{
    Task AddMTNReportToQueue(MTNReport report);
}

public class MTNReportPublisherService : IMTNReportPublisherService
{
    private readonly ServiceBusSender _sender;

    public MTNReportPublisherService(IConfiguration configuration, ServiceBusClient serviceBusClient)
    {
        _sender = serviceBusClient.CreateSender(configuration["SERVICEBUS_MTNREPORTQUEUE"]);
    }

    public async Task AddMTNReportToQueue(MTNReport report)
    {
        var message = new ServiceBusMessage(JsonConvert.SerializeObject(report))
        {
            ContentType = "application/json"
        };
        await _sender.SendMessageAsync(message);
    }
}
