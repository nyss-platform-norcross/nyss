using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Reports.Dto;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Reports
{
    public interface IReportSenderService
    {
        Task<Result> SendReport(SendReportRequestDto report);
    }

    public class ReportSenderService : IReportSenderService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssWebConfig _config;

        public ReportSenderService(IHttpClientFactory httpClientFactory, ILoggerAdapter loggerAdapter, INyssWebConfig config)
        {
            _httpClientFactory = httpClientFactory;
            _loggerAdapter = loggerAdapter;
            _config = config;
        }

        public async Task<Result> SendReport(SendReportRequestDto report)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            var baseUri = new Uri(_config.FuncAppBaseUrl);
            var requestUri = new Uri(baseUri, new Uri("/api/enqueueSmsEagleReport", UriKind.Relative));
            var reportProps = new Dictionary<string, string>
            {
                { "Sender", report.Sender },
                { "Timestamp", report.Timestamp },
                { "ApiKey", report.ApiKey },
                { "Text", report.Text },
                { "Source", "Nyss" }
            };
            var content = new FormUrlEncodedContent(reportProps);

            var responseMessage = await httpClient.PostAsync(requestUri, content);

            responseMessage.EnsureSuccessStatusCode();

            return SuccessMessage(ResultKey.Report.ReportSentSuccessfully);
        }
    }
}