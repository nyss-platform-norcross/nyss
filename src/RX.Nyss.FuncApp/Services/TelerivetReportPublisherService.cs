using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RX.Nyss.FuncApp.Contracts;

namespace RX.Nyss.FuncApp.Services;

public interface ITelerivetReportPublisherService
{
    Task AddTelerivetReportToQueue(TelerivetReport report);
}

public class TelerivetReportPublisherService : ITelerivetReportPublisherService
{
    private readonly ServiceBusSender _sender;

    public TelerivetReportPublisherService(IConfiguration configuration, ServiceBusClient serviceBusClient)
    {
        _sender = serviceBusClient.CreateSender(configuration["SERVICEBUS_TELERIVETREPORTQUEUE"]);
    }

    public async Task AddTelerivetReportToQueue(TelerivetReport report)
    {
        var message = new ServiceBusMessage(JsonConvert.SerializeObject(report))
        {
            ContentType = "application/json"
        };
        await _sender.SendMessageAsync(message);
    }
}
