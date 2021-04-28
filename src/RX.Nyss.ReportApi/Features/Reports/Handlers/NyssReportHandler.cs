using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Features.Alerts;
using RX.Nyss.ReportApi.Features.Common;
using RX.Nyss.ReportApi.Features.Reports.Exceptions;
using RX.Nyss.ReportApi.Features.Reports.Models;
using RX.Nyss.ReportApi.Services;
using Report = RX.Nyss.Data.Models.Report;

namespace RX.Nyss.ReportApi.Features.Reports.Handlers
{
    public interface INyssReportHandler
    {
        Task Handle(string queryString);
    }

    public class NyssReportHandler : INyssReportHandler
    {
        private const string DataCollectorIdParameterName = "datacollectorid";
        private const string TimestampParameterName = "timestamp";
        private const string TextParameterName = "text";
        private const string SentByHeadSupervisorSourceParameterName = "headsupervisor";
        private const string ModemNumberParameterName = "modemno";
        private const string UtcOffset = "utcoffset";
        private const string ApiKeyParameterName = "apikey";


        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IReportMessageService _reportMessageService;
        private readonly IReportValidationService _reportValidationService;
        private readonly IAlertService _alertService;
        private readonly IStringsResourcesService _stringsResourcesService;
        private readonly IQueuePublisherService _queuePublisherService;

        public NyssReportHandler(
            INyssContext nyssContext,
            ILoggerAdapter loggerAdapter,
            IDateTimeProvider dateTimeProvider,
            IReportMessageService reportMessageService,
            IReportValidationService reportValidationService,
            IAlertService alertService,
            IStringsResourcesService stringsResourcesService,
            IQueuePublisherService queuePublisherService)
        {
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
            _dateTimeProvider = dateTimeProvider;
            _reportMessageService = reportMessageService;
            _reportValidationService = reportValidationService;
            _alertService = alertService;
            _stringsResourcesService = stringsResourcesService;
            _queuePublisherService = queuePublisherService;
        }

        public async Task Handle(string queryString)
        {
            var parsedQueryString = HttpUtility.ParseQueryString(queryString);
            var timestamp = parsedQueryString[TimestampParameterName];
            var text = parsedQueryString[TextParameterName].Trim();
            var sentByHeadSupervisor = parsedQueryString[SentByHeadSupervisorSourceParameterName] == "true";
            var modemNumber = parsedQueryString[ModemNumberParameterName].ParseToNullableInt();
            var utcOffset = parsedQueryString[UtcOffset].ParseToNullableInt();
            var apiKey = parsedQueryString[ApiKeyParameterName];

            DataCollector dataCollector = null;
            Report report = null;
            AlertData alertData = null;
            GatewaySetting gatewaySetting = null;
            ErrorReportData reportErrorData = null;

            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                var rawReport = new RawReport
                {
                    Timestamp = timestamp,
                    ReceivedAt = _dateTimeProvider.UtcNow,
                    Text = text,
                    ModemNumber = modemNumber,
                    ApiKey = apiKey
                };

                await _nyssContext.AddAsync(rawReport);

                try
                {
                    var reportValidationResult = await ParseAndValidateReport(rawReport, parsedQueryString);
                    if (reportValidationResult.IsSuccess)
                    {
                        dataCollector = reportValidationResult.ReportData.DataCollector;
                        gatewaySetting = reportValidationResult.GatewaySetting;
                        var parsedReport = reportValidationResult.ReportData.ParsedReport;
                        var projectHealthRisk = reportValidationResult.ReportData.ProjectHealthRisk;

                        var epiDate = _dateTimeProvider.GetEpiDate(rawReport.ReceivedAt);

                        report = new Report
                        {
                            IsTraining = dataCollector.IsInTrainingMode,
                            ReportType = parsedReport.ReportType,
                            Status = ReportStatus.New,
                            ReceivedAt = reportValidationResult.ReportData.ReceivedAt,
                            CreatedAt = _dateTimeProvider.UtcNow,
                            DataCollector = dataCollector,
                            EpiWeek = epiDate.EpiWeek,
                            EpiYear = epiDate.EpiYear,
                            Location = dataCollector.DataCollectorLocations.Count == 1
                                ? dataCollector.DataCollectorLocations.First().Location
                                : null,
                            ReportedCase = parsedReport.ReportedCase,
                            DataCollectionPointCase = parsedReport.DataCollectionPointCase,
                            ProjectHealthRisk = projectHealthRisk,
                            ReportedCaseCount = projectHealthRisk.HealthRisk.HealthRiskType == HealthRiskType.Human
                                ? (parsedReport.ReportedCase.CountFemalesAtLeastFive ?? 0)
                                + (parsedReport.ReportedCase.CountFemalesBelowFive ?? 0)
                                + (parsedReport.ReportedCase.CountMalesAtLeastFive ?? 0)
                                + (parsedReport.ReportedCase.CountMalesBelowFive ?? 0)
                                + (parsedReport.ReportedCase.CountUnspecifiedSexAndAge ?? 0)
                                : 1
                        };

                        rawReport.Report = report;
                        await _nyssContext.Reports.AddAsync(report);
                        alertData = await _alertService.ReportAdded(report);
                    }
                    else
                    {
                        rawReport.ErrorType = reportValidationResult.ErrorReportData.ReportErrorType;
                        reportErrorData = reportValidationResult.ErrorReportData;
                    }
                }
                catch (ReportValidationException e)
                {
                    _loggerAdapter.Warn(e.Message);
                    rawReport.ErrorType = e.ErrorType;
                    reportErrorData = new ErrorReportData { ReportErrorType = e.ErrorType };
                }

                await _nyssContext.SaveChangesAsync();
                transactionScope.Complete();
            }

            if (reportErrorData == null)
            {
                await HandleFeedbackMessageToDataCollector(dataCollector, gatewaySetting, report, modemNumber, sentByHeadSupervisor, utcOffset);
                await HandleAlertNotifications(alertData, gatewaySetting);
            }
        }

        private async Task<ReportValidationResult> ParseAndValidateReport(RawReport rawReport, NameValueCollection parsedQueryString)
        {
            try
            {
                var dataCollectorId = int.Parse(parsedQueryString[DataCollectorIdParameterName]);
                var timestamp = parsedQueryString[TimestampParameterName];
                var text = parsedQueryString[TextParameterName].Trim();
                var modemNumber = parsedQueryString[ModemNumberParameterName].ParseToNullableInt();
                var apiKey = parsedQueryString[ApiKeyParameterName];

                var receivedAt = _reportValidationService.ParseTimestamp(timestamp);
                _reportValidationService.ValidateReceivalTime(receivedAt);
                rawReport.ReceivedAt = receivedAt;

                var gatewaySetting = await _reportValidationService.ValidateGatewaySetting(apiKey);

                var dataCollector = await ValidateDataCollector(dataCollectorId, gatewaySetting.NationalSocietyId);
                rawReport.DataCollector = dataCollector;
                rawReport.NationalSociety = dataCollector?.Project.NationalSociety ?? gatewaySetting.NationalSociety;
                rawReport.IsTraining = dataCollector?.IsInTrainingMode ?? false;
                rawReport.Village = dataCollector?.DataCollectorLocations.Count == 1
                    ? dataCollector.DataCollectorLocations.First().Village
                    : null;
                rawReport.Zone = dataCollector?.DataCollectorLocations.Count == 1
                    ? dataCollector.DataCollectorLocations.First().Zone
                    : null;

                var parsedReport = await _reportMessageService.ParseReport(text);
                var projectHealthRisk = await _reportValidationService.ValidateReport(parsedReport, dataCollector, gatewaySetting.NationalSocietyId);

                return new ReportValidationResult
                {
                    IsSuccess = true,
                    ReportData = new ReportData
                    {
                        DataCollector = dataCollector,
                        ParsedReport = parsedReport,
                        ProjectHealthRisk = projectHealthRisk,
                        ModemNumber = modemNumber
                    },
                    GatewaySetting = gatewaySetting
                };
            }
            catch (ReportValidationException e)
            {
                return new ReportValidationResult
                {
                    IsSuccess = false,
                    ErrorReportData = new ErrorReportData { ReportErrorType = e.ErrorType }
                };
            }
        }

        private async Task HandleFeedbackMessageToDataCollector(DataCollector dataCollector, GatewaySetting gatewaySetting, Report report, int? modemNumber, bool sentByHeadSupervisor, int? utcOffset)
        {
            if (dataCollector == null || string.IsNullOrEmpty(dataCollector.PhoneNumber))
            {
                return;
            }

            var recipients = new List<SendSmsRecipient>
            {
                new SendSmsRecipient
                {
                    PhoneNumber = dataCollector.PhoneNumber,
                    Modem = modemNumber
                }
            };

            var feedbackForReportSentThroughNyss = await GetFeedbackMessage(report, dataCollector, gatewaySetting, sentByHeadSupervisor, utcOffset);
            var feedbackMessage = !string.IsNullOrEmpty(feedbackForReportSentThroughNyss)
                ? $"{feedbackForReportSentThroughNyss} {report.ProjectHealthRisk.FeedbackMessage}"
                : report.ProjectHealthRisk.FeedbackMessage;
            await _queuePublisherService.SendSms(recipients, gatewaySetting, feedbackMessage);
        }

        private async Task HandleAlertNotifications(AlertData alertData, GatewaySetting gatewaySetting)
        {
            if (alertData != null && alertData.Alert != null)
            {
                if (alertData.IsExistingAlert)
                {
                    await _alertService.SendNotificationsForSupervisorsAddedToExistingAlert(alertData.Alert, alertData.SupervisorsAddedToExistingAlert, gatewaySetting);
                }
                else
                {
                    await _alertService.SendNotificationsForNewAlert(alertData.Alert, gatewaySetting);
                }
            }
        }

        private async Task<DataCollector> ValidateDataCollector(int dataCollectorId, int nationalSocietyId)
        {
            var dataCollector = await _nyssContext.DataCollectors
                .Include(dc => dc.Supervisor)
                .ThenInclude(s => s.HeadSupervisor)
                .Include(dc => dc.Project)
                .ThenInclude(p => p.NationalSociety)
                .Include(dc => dc.DataCollectorLocations)
                .ThenInclude(dc => dc.Village)
                .Include(dc => dc.DataCollectorLocations)
                .ThenInclude(dc => dc.Zone)
                .Where(dc => dc.Id == dataCollectorId)
                .SingleOrDefaultAsync();

            if (dataCollector.Project.NationalSocietyId != nationalSocietyId)
            {
                throw new ReportValidationException($"A Data Collector's National Society identifier ('{dataCollector.Project.NationalSocietyId}') " +
                    $"is different from SMS Gateway's ('{nationalSocietyId}').");
            }

            return dataCollector;
        }

        private async Task<string> GetFeedbackMessage(Report report, DataCollector dataCollector, GatewaySetting gatewaySetting, bool sentByHeadSupervisor, int? utcOffset)
        {
            var languageCode = await _nyssContext.NationalSocieties
                .Where(ns => ns.Id == gatewaySetting.NationalSocietyId)
                .Select(ns => ns.ContentLanguage.LanguageCode)
                .FirstOrDefaultAsync();

            var feedbackMessage = await GetFeedbackMessageContent(SmsContentKey.Reports.ReportSentFromNyss, languageCode);
            var senderName = sentByHeadSupervisor ? dataCollector.Supervisor.HeadSupervisor.Name : dataCollector.Supervisor.Name;

            var languageContents = await _nyssContext.HealthRiskLanguageContents
                .SingleAsync(hlc => hlc.HealthRisk == report.ProjectHealthRisk.HealthRisk && hlc.ContentLanguage.LanguageCode == languageCode);

            var timestamp = utcOffset.HasValue
                ? report.ReceivedAt.AddHours(utcOffset.Value).ToString("yyyy-MM-dd HH:mm")
                : report.ReceivedAt.ToString("yyyy-MM-dd HH:mm");

            feedbackMessage = feedbackMessage.Replace("{{supervisor}}", senderName);
            feedbackMessage = feedbackMessage.Replace("{{date/time}}", timestamp);
            feedbackMessage = feedbackMessage.Replace("{{health risk/event}}", languageContents.Name);

            return feedbackMessage;
        }

        private async Task<string> GetFeedbackMessageContent(string key, string languageCode)
        {
            var smsContents = await _stringsResourcesService.GetSmsContentResources(!string.IsNullOrEmpty(languageCode)
                ? languageCode
                : "en");
            smsContents.Value.TryGetValue(key, out var message);

            if (message == null)
            {
                _loggerAdapter.Warn($"No sms content resource found for key '{key}'");
            }

            return message;
        }
    }
}
