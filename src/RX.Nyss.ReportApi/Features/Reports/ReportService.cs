using System;
using System.Threading.Tasks;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.ReportApi.Features.Reports.Contracts;
using RX.Nyss.ReportApi.Features.Reports.Handlers;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.ReportApi.Features.Alerts;

namespace RX.Nyss.ReportApi.Features.Reports
{
    public interface IReportService
    {
        Task<bool> ReceiveReport(Report report);
    }

    public class ReportService : IReportService
    {
        private readonly ISmsEagleHandler _smsEagleHandler;
        private readonly INyssReportHandler _nyssReportHandler;
        private readonly ILoggerAdapter _loggerAdapter;

        public ReportService(ISmsEagleHandler smsEagleHandler, ILoggerAdapter loggerAdapter, INyssReportHandler nyssReportHandler)
        {
            _smsEagleHandler = smsEagleHandler;
            _loggerAdapter = loggerAdapter;
            _nyssReportHandler = nyssReportHandler;
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
                case ReportSource.SmsEagle:
                    await _smsEagleHandler.Handle(report.Content);
                    break;
                case ReportSource.Nyss:
                    await _nyssReportHandler.Handle(report.Content);
                    break;
                default:
                    _loggerAdapter.Error($"Could not find a proper handler to handle a report '{report}'.");
                    break;
            }

            return true;
        }
    }
}
