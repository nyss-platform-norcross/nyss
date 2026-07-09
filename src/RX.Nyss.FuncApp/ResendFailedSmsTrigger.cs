using Microsoft.Extensions.Logging;
using RX.Nyss.FuncApp.Services;
using Microsoft.Azure.Functions.Worker;

namespace RX.Nyss.FuncApp;

public class ResendFailedSmsTrigger
{
    private readonly IDeadLetterSmsService _deadLetterSmsService;

    public ResendFailedSmsTrigger(IDeadLetterSmsService deadLetterSmsService)
    {
        _deadLetterSmsService = deadLetterSmsService;
    }

    [Function("ResendFailedSmsTrigger")]
    public async Task RunAsync([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer, ILogger log) =>
        await _deadLetterSmsService.ResubmitDeadLetterMessages();
}