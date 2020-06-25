using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
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
        private readonly INyssContext _nyssContext;

        public ReportSenderService(IHttpClientFactory httpClientFactory, ILoggerAdapter loggerAdapter, INyssWebConfig config, INyssContext nyssContext)
        {
            _httpClientFactory = httpClientFactory;
            _loggerAdapter = loggerAdapter;
            _config = config;
            _nyssContext = nyssContext;
        }

        public async Task<Result> SendReport(SendReportRequestDto report)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            var nationalSociety = await _nyssContext.DataCollectors
                .Where(dc => dc.PhoneNumber == report.Sender)
                .Select(dc => dc.Project.NationalSociety)
                .SingleOrDefaultAsync();
            var apiKey = await _nyssContext.GatewaySettings
                .Where(gs => gs.NationalSociety == nationalSociety)
                .Select(gs => gs.ApiKey)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(apiKey))
            {
                return Error(ResultKey.Report.NoGatewaySettingFoundForNationalSociety);
            }

            var baseUri = new Uri(_config.FuncAppBaseUrl);
            var requestUri = new Uri(baseUri, new Uri("/api/enqueueSmsEagleReport", UriKind.Relative));
            var reportProps = new Dictionary<string, string>
            {
                { "Sender", report.Sender },
                { "Timestamp", report.Timestamp },
                { "ApiKey", apiKey },
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