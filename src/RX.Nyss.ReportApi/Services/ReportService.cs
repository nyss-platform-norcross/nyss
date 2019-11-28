using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RX.Nyss.ReportApi.Handlers;
using RX.Nyss.ReportApi.Utils.Logging;

namespace RX.Nyss.ReportApi.Services
{
    public interface IReportService
    {
        Task<bool> ReceiveSms(string sms);
    }

    public class ReportService : IReportService
    {
        private readonly IEnumerable<ISmsHandler> _smsHandlers;
        private readonly ILoggerAdapter _loggerAdapter;

        public ReportService(IEnumerable<ISmsHandler> smsHandlers, ILoggerAdapter loggerAdapter)
        {
            _smsHandlers = smsHandlers;
            _loggerAdapter = loggerAdapter;
        }

        public async Task<bool> ReceiveSms(string sms)
        {
            if (sms == null)
            {
                _loggerAdapter.Error("Received SMS with null value.");
                return false;
            }

            _loggerAdapter.Debug($"Received SMS: {sms}");

            var smsHandlers = _smsHandlers.Where(h => h.CanHandle(sms)).ToList();

            switch (smsHandlers.Count)
            {
                case 0: _loggerAdapter.Error($"Could not find a handler to handle SMS message ({sms}).");
                    break;
                case 1: await _smsHandlers.Single().Handle(sms);
                    break;
                default: _loggerAdapter.Error($"Found many handlers ({smsHandlers.Select(h => h.GetType().Name)}) which can handle SMS message ({sms}).");                          
                    break;
            }

            return true;
        }
    }
}
