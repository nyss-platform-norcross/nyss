using System;
using System.Threading.Tasks;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.ReportApi.Features.Reports.Contracts;
using RX.Nyss.ReportApi.Features.Reports.Handlers;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.ReportApi.Features.Reports.Models;

namespace RX.Nyss.ReportApi.Features.Reports
{
    public interface IReportService
    {
        Task<bool> ReceiveReport(Report report);
        Task<bool> RegisterEidsrEvent(EidsrReport eidsrReport);
    }

    public class ReportService : IReportService
    {
        private readonly ISmsEagleHandler _smsEagleHandler;
        private readonly INyssReportHandler _nyssReportHandler;
        private readonly ISmsGatewayHandler _smsGatewayHandler;
        private readonly IEidsrReportHandler _eidsrReportHandler;
        private readonly ILoggerAdapter _loggerAdapter;

        public ReportService(
            ISmsEagleHandler smsEagleHandler,
            ILoggerAdapter loggerAdapter,
            INyssReportHandler nyssReportHandler,
            IEidsrReportHandler eidsrReportHandler,
            ISmsGatewayHandler smsGatewayHandler)
        {
            _smsEagleHandler = smsEagleHandler;
            _loggerAdapter = loggerAdapter;
            _nyssReportHandler = nyssReportHandler;
            _eidsrReportHandler = eidsrReportHandler;
            //_nyssReportHandler = nyssReportHandler;
            _smsGatewayHandler = smsGatewayHandler;
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
                case ReportSource.SmsGateway:
                    await _smsGatewayHandler.Handle(report.Content);
                    break;
                default:
                    _loggerAdapter.Error($"Could not find a proper handler to handle a report '{report}'.");
                    break;
            }

            return true;
        }

        public async Task<bool> RegisterEidsrEvent(EidsrReport eidsrReport)
        {
            if (eidsrReport == null)
            {
                _loggerAdapter.Error("Received a eidsrReport with null value.");
                return false;
            }

            return await _eidsrReportHandler.Handle(eidsrReport);
        }
    }
}
