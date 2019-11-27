﻿using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using RX.Nyss.ReportFuncApp.Configuration;
using RX.Nyss.ReportFuncApp.Models;

namespace RX.Nyss.ReportFuncApp
{
    public class ReportApiTrigger
    {
        private readonly ILogger<ReportApiTrigger> _logger;
        private readonly INyssReportFuncAppConfig _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public ReportApiTrigger(ILogger<ReportApiTrigger> logger, INyssReportFuncAppConfig configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [FunctionName("DequeueReport")]
        public async Task DequeueReport(
            [ServiceBusTrigger("%SERVICEBUS_REPORTQUEUE%", Connection = "SERVICEBUS_CONNECTIONSTRING")] string report)
        {
            _logger.Log(LogLevel.Debug, $"Dequeued report: '{report}'");

            var client = _httpClientFactory.CreateClient();
            var postResult = await client.PostAsync(_configuration.ReportApiUrl, new Sms { Content = report }, new JsonMediaTypeFormatter());

            if (!postResult.IsSuccessStatusCode)
            {
                _logger.LogError($"Status code: {(int) postResult.StatusCode} ReasonPhrase: {postResult.ReasonPhrase}");
                throw new Exception($"A report '{report}' was not handled properly by the Report API.");
            }
        }
    }
}
