using System.Threading.Tasks;
using RX.Nyss.ReportApi.Contracts;
using RX.Nyss.ReportApi.Handlers;
using RX.Nyss.ReportApi.Utils.Logging;

namespace RX.Nyss.ReportApi.Services
{
    public interface IReportService
    {
        Task<bool> ReceiveReport(Report report);
    }

    public class ReportService : IReportService
    {
        private readonly ISmsEagleHandler _smsEagleHandler;
        private readonly ILoggerAdapter _loggerAdapter;

        public ReportService(ISmsEagleHandler smsEagleHandler, ILoggerAdapter loggerAdapter)
        {
            _smsEagleHandler = smsEagleHandler;
            _loggerAdapter = loggerAdapter;
        }

        public async Task<bool> ReceiveReport(Report report)
        {
            if (report == null)
            {
                _loggerAdapter.Error("Received a report with null value.");
                return false;
            }

            _loggerAdapter.Debug($"Received report: {report}");

            switch (report.ReportSource)
            {
                case ReportSource.SmsEagle: await _smsEagleHandler.Handle(report.Content);
                    break;
                default: _loggerAdapter.Error($"Could not find a proper handler to handle a report '{report}'.");                       
                    break;
            }

            return true;
        }
    }
}
