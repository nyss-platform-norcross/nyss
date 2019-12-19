using System;
using System.Threading.Tasks;
using RX.Nyss.ReportApi.Features.Alerts;
using RX.Nyss.ReportApi.Features.Reports.Contracts;
using RX.Nyss.ReportApi.Features.Reports.Handlers;
using RX.Nyss.ReportApi.Utils.Logging;

namespace RX.Nyss.ReportApi.Features.Reports
{
    public interface IReportService
    {
        Task<bool> ReceiveReport(Report report);
        Task<bool> DismissReport(int reportId);
    }

    public class ReportService : IReportService
    {
        private readonly ISmsEagleHandler _smsEagleHandler;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IAlertService _alertService;

        public ReportService(ISmsEagleHandler smsEagleHandler, ILoggerAdapter loggerAdapter, IAlertService alertService)
        {
            _smsEagleHandler = smsEagleHandler;
            _loggerAdapter = loggerAdapter;
            _alertService = alertService;
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

        public async Task<bool> DismissReport(int reportId)
        {
            try
            {
                await _alertService.ReportDismissed(reportId);
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}
