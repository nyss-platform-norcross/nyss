using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using RX.Nyss.FuncApp.Contracts;
using RX.Nyss.FuncApp.Services;

namespace RX.Nyss.FuncApp;

public class SendSmsTrigger
{
    private readonly ISmsService _smsService;

    public SendSmsTrigger(ISmsService smsService)
    {
        _smsService = smsService;
    }

    [FunctionName("SendSms")]
    public async Task SendSms(
        [ServiceBusTrigger("%SERVICEBUS_SENDSMSQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] SendSmsMessage message,
        [Blob("%WhitelistedPhoneNumbersBlobPath%", FileAccess.Read)] string whitelistedPhoneNumbers) =>
        await _smsService.SendSms(message, whitelistedPhoneNumbers);
}