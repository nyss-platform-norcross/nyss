using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using RX.Nyss.FuncApp.Services;

namespace RX.Nyss.FuncApp;

public class ResendFailedSmsTrigger
{
    private readonly IDeadLetterSmsService _deadLetterSmsService;

    public ResendFailedSmsTrigger(IDeadLetterSmsService deadLetterSmsService)
    {
        _deadLetterSmsService = deadLetterSmsService;
    }

    [FunctionName("ResendFailedSmsTrigger")]
    public async Task RunAsync([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer, ILogger log) =>
        await _deadLetterSmsService.ResubmitDeadLetterMessages();
}