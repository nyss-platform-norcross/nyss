using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
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
using RX.Nyss.ReportApi.Features.Reports.Contracts;
using RX.Nyss.ReportApi.Features.Reports.Exceptions;
using RX.Nyss.ReportApi.Features.Reports.Models;
using RX.Nyss.ReportApi.Services;
using Report = RX.Nyss.Data.Models.Report;

namespace RX.Nyss.ReportApi.Features.Reports.Handlers
{
    public interface ISmsEagleHandler
    {
        Task Handle(string queryString);
        Task<GatewaySetting> ValidateGatewaySetting(string apiKey);
        Task<DataCollector> ValidateDataCollector(string phoneNumber, int gatewayNationalSocietyId);
    }

    public class SmsEagleHandler : ISmsEagleHandler
    {
        private const string SenderParameterName = "sender";
        private const string TimestampParameterName = "timestamp";
        private const string TextParameterName = "text";
        private const string IncomingMessageIdParameterName = "msgid";
        private const string OutgoingMessageIdParameterName = "oid";
        private const string ModemNumberParameterName = "modemno";
        private const string ApiKeyParameterName = "apikey";

        private readonly IReportMessageService _reportMessageService;
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IQueuePublisherService _queuePublisherService;
        private readonly IAlertService _alertService;
        private readonly IStringsResourcesService _stringsResourcesService;
        private readonly IReportValidationService _reportValidationService;

        public SmsEagleHandler(
            IReportMessageService reportMessageService,
            INyssContext nyssContext,
            ILoggerAdapter loggerAdapter,
            IDateTimeProvider dateTimeProvider,
            IStringsResourcesService stringsResourcesService,
            IQueuePublisherService queuePublisherService,
            IAlertService alertService,
            IReportValidationService reportValidationService)
        {
            _reportMessageService = reportMessageService;
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
            _dateTimeProvider = dateTimeProvider;
            _queuePublisherService = queuePublisherService;
            _alertService = alertService;
            _stringsResourcesService = stringsResourcesService;
            _reportValidationService = reportValidationService;
        }

        public async Task Handle(string queryString)
        {
            var parsedQueryString = HttpUtility.ParseQueryString(queryString);
            var sender = parsedQueryString[SenderParameterName];
            var timestamp = parsedQueryString[TimestampParameterName];
            var text = parsedQueryString[TextParameterName].Trim();
            var incomingMessageId = parsedQueryString[IncomingMessageIdParameterName].ParseToNullableInt();
            var outgoingMessageId = parsedQueryString[OutgoingMessageIdParameterName].ParseToNullableInt();
            var modemNumber = parsedQueryString[ModemNumberParameterName].ParseToNullableInt();
            var apiKey = parsedQueryString[ApiKeyParameterName];

            ErrorReportData reportErrorData = null;

            try
            {
                AlertData alertData = null;
                ProjectHealthRisk projectHealthRisk = null;
                GatewaySetting gatewaySetting = null;
                ReportData reportData = null;

                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var rawReport = new RawReport
                    {
                        Sender = sender,
                        Timestamp = timestamp,
                        ReceivedAt = _dateTimeProvider.UtcNow,
                        Text = text.Truncate(160),
                        IncomingMessageId = incomingMessageId,
                        OutgoingMessageId = outgoingMessageId,
                        ModemNumber = modemNumber,
                        ApiKey = apiKey
                    };
                    await _nyssContext.AddAsync(rawReport);

                    var reportValidationResult = await ParseAndValidateReport(rawReport, parsedQueryString);
                    if (reportValidationResult.IsSuccess)
                    {
                        gatewaySetting = reportValidationResult.GatewaySetting;
                        projectHealthRisk = reportValidationResult.ReportData.ProjectHealthRisk;
                        reportData = reportValidationResult.ReportData;

                        var epiDate = _dateTimeProvider.GetEpiDate(reportValidationResult.ReportData.ReceivedAt);

                        var report = new Report
                        {
                            IsTraining = reportValidationResult.ReportData.DataCollector.IsInTrainingMode,
                            ReportType = reportValidationResult.ReportData.ParsedReport.ReportType,
                            Status = ReportStatus.New,
                            ReceivedAt = reportValidationResult.ReportData.ReceivedAt,
                            CreatedAt = _dateTimeProvider.UtcNow,
                            DataCollector = reportValidationResult.ReportData.DataCollector,
                            EpiWeek = epiDate.EpiWeek,
                            EpiYear = epiDate.EpiYear,
                            PhoneNumber = sender,
                            Location = reportValidationResult.ReportData.DataCollector.DataCollectorLocations.Count == 1
                                ? reportValidationResult.ReportData.DataCollector.DataCollectorLocations.First().Location
                                : null,
                            ReportedCase = reportValidationResult.ReportData.ParsedReport.ReportedCase,
                            DataCollectionPointCase = reportValidationResult.ReportData.ParsedReport.DataCollectionPointCase,
                            ProjectHealthRisk = projectHealthRisk,
                            ReportedCaseCount = projectHealthRisk.HealthRisk.HealthRiskType == HealthRiskType.Human
                                ? (reportValidationResult.ReportData.ParsedReport.ReportedCase.CountFemalesAtLeastFive ?? 0)
                                + (reportValidationResult.ReportData.ParsedReport.ReportedCase.CountFemalesBelowFive ?? 0)
                                + (reportValidationResult.ReportData.ParsedReport.ReportedCase.CountMalesAtLeastFive ?? 0)
                                + (reportValidationResult.ReportData.ParsedReport.ReportedCase.CountMalesBelowFive ?? 0)
                                + (reportValidationResult.ReportData.ParsedReport.ReportedCase.CountUnspecifiedSexAndAge ?? 0)
                                : 1
                        };

                        rawReport.Report = report;
                        await _nyssContext.Reports.AddAsync(report);
                        alertData = await _alertService.ReportAdded(report);
                    }
                    else
                    {
                        reportErrorData = reportValidationResult.ErrorReportData;
                        gatewaySetting = reportValidationResult.GatewaySetting;
                    }

                    await _nyssContext.SaveChangesAsync();
                    transactionScope.Complete();
                }

                if (reportErrorData == null)
                {
                    if (projectHealthRisk != null)
                    {
                        var recipients = new List<SendSmsRecipient>
                        {
                            new SendSmsRecipient
                            {
                                PhoneNumber = sender,
                                Modem = modemNumber
                            }
                        };

                        await _queuePublisherService.SendSms(recipients, gatewaySetting, projectHealthRisk.FeedbackMessage);
                    }

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
                else
                {
                    await SendFeedbackOnError(reportErrorData, gatewaySetting);
                }
            }
            catch (ReportValidationException e)
            {
                _loggerAdapter.Warn(e.Message);
            }
        }

        public async Task<GatewaySetting> ValidateGatewaySetting(string apiKey)
        {
            var gatewaySetting = await _nyssContext.GatewaySettings
                .Include(gs => gs.NationalSociety)
                .Include(gs => gs.Modems)
                .SingleOrDefaultAsync(gs => gs.ApiKey == apiKey);

            if (gatewaySetting == null)
            {
                throw new ReportValidationException($"A gateway setting with API key '{apiKey}' does not exist.");
            }

            if (gatewaySetting.GatewayType != GatewayType.SmsEagle)
            {
                throw new ReportValidationException($"A gateway type ('{gatewaySetting.GatewayType}') is different than '{GatewayType.SmsEagle}'.");
            }

            return gatewaySetting;
        }

        public async Task<DataCollector> ValidateDataCollector(string phoneNumber, int gatewayNationalSocietyId)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ReportValidationException("A phone number cannot be empty.");
            }

            var dataCollector = await _nyssContext.DataCollectors
                .Include(dc => dc.Supervisor)
                .ThenInclude(s => s.HeadSupervisor)
                .Include(dc => dc.Project)
                .Include(dc => dc.DataCollectorLocations)
                .ThenInclude(dcl => dcl.Village)
                .Include(dc => dc.DataCollectorLocations)
                .ThenInclude(dcl => dcl.Zone)
                .SingleOrDefaultAsync(dc => dc.PhoneNumber == phoneNumber ||
                    (dc.AdditionalPhoneNumber != null && dc.AdditionalPhoneNumber == phoneNumber));

            if (dataCollector == null)
            {
                throw new ReportValidationException($"A Data Collector with phone number '{phoneNumber}' does not exist.", ReportErrorType.DataCollectorNotFound);
            }

            if (dataCollector.Project.NationalSocietyId != gatewayNationalSocietyId)
            {
                throw new ReportValidationException($"A Data Collector's National Society identifier ('{dataCollector.Project.NationalSocietyId}') " +
                    $"is different from SMS Gateway's ('{gatewayNationalSocietyId}').", ReportErrorType.DataCollectorNotFound);
            }

            return dataCollector;
        }

        private async Task<ReportValidationResult> ParseAndValidateReport(RawReport rawReport, NameValueCollection parsedQueryString)
        {
            GatewaySetting gatewaySetting = null;
            try
            {
                var apiKey = parsedQueryString[ApiKeyParameterName];
                var sender = parsedQueryString[SenderParameterName];
                var timestamp = parsedQueryString[TimestampParameterName];
                var text = parsedQueryString[TextParameterName].Trim();

                var receivedAt = _reportValidationService.ParseTimestamp(timestamp);
                _reportValidationService.ValidateReceivalTime(receivedAt);
                rawReport.ReceivedAt = receivedAt;

                gatewaySetting = await ValidateGatewaySetting(apiKey);
                rawReport.NationalSociety = gatewaySetting.NationalSociety;

                var dataCollector = await ValidateDataCollector(sender, gatewaySetting.NationalSocietyId);
                rawReport.DataCollector = dataCollector;
                rawReport.IsTraining = dataCollector.IsInTrainingMode;
                rawReport.Village = dataCollector.DataCollectorLocations.Count == 1 ? dataCollector.DataCollectorLocations.First().Village : null;
                rawReport.Zone = dataCollector.DataCollectorLocations.Count == 1 ? dataCollector.DataCollectorLocations.First().Zone : null;

                var parsedReport = await _reportMessageService.ParseReport(text);
                var projectHealthRisk = await _reportValidationService.ValidateReport(parsedReport, dataCollector);

                return new ReportValidationResult
                {
                    IsSuccess = true,
                    ReportData = new ReportData
                    {
                        DataCollector = dataCollector,
                        ProjectHealthRisk = projectHealthRisk,
                        ReceivedAt = receivedAt,
                        ParsedReport = parsedReport,
                        ModemNumber = rawReport.ModemNumber
                    },
                    GatewaySetting = gatewaySetting
                };
            }
            catch (ReportValidationException e)
            {
                _loggerAdapter.Warn(e);

                var sender = parsedQueryString[SenderParameterName];

                string languageCode = null;
                if (gatewaySetting != null)
                {
                    languageCode = await _nyssContext.NationalSocieties
                        .Where(ns => ns.Id == gatewaySetting.NationalSocietyId)
                        .Select(ns => ns.ContentLanguage.LanguageCode)
                        .FirstOrDefaultAsync();
                }

                return new ReportValidationResult
                {
                    IsSuccess = false,
                    ErrorReportData = new ErrorReportData
                    {
                        Sender = sender,
                        LanguageCode = languageCode,
                        ReportErrorType = e.ErrorType,
                        ModemNumber = rawReport.ModemNumber
                    },
                    GatewaySetting = gatewaySetting
                };
            }
            catch (Exception e)
            {
                _loggerAdapter.Warn(e.Message);
                return new ReportValidationResult { IsSuccess = false };
            }
        }

        private async Task<string> GetFeedbackMessageForReportSentThroughNyss(ReportData reportData, GatewaySetting gatewaySetting, bool sentByHeadSupervisor, int? utcOffset)
        {
            var languageCode = await _nyssContext.NationalSocieties
                .Where(ns => ns.Id == gatewaySetting.NationalSocietyId)
                .Select(ns => ns.ContentLanguage.LanguageCode)
                .FirstOrDefaultAsync();

            var feedbackMessage = await GetFeedbackMessageContent(SmsContentKey.Reports.ReportSentFromNyss, languageCode);
            var senderName = sentByHeadSupervisor ? reportData.DataCollector.Supervisor.HeadSupervisor.Name : reportData.DataCollector.Supervisor.Name;

            var languageContents = await _nyssContext.HealthRiskLanguageContents
                .SingleAsync(hlc => hlc.HealthRisk == reportData.ProjectHealthRisk.HealthRisk && hlc.ContentLanguage.LanguageCode == languageCode);

            var timestamp = utcOffset.HasValue
                ? reportData.ReceivedAt.AddHours(utcOffset.Value).ToString("yyyy-MM-dd HH:mm")
                : reportData.ReceivedAt.ToString("yyyy-MM-dd HH:mm");

            feedbackMessage = feedbackMessage.Replace("{{supervisor}}", senderName);
            feedbackMessage = feedbackMessage.Replace("{{date/time}}", timestamp);
            feedbackMessage = feedbackMessage.Replace("{{health risk/event}}", languageContents.Name);

            return feedbackMessage;
        }

        private async Task SendFeedbackOnError(ErrorReportData errorReport, GatewaySetting gatewaySetting)
        {
            if (gatewaySetting != null && !string.IsNullOrEmpty(errorReport.Sender))
            {
                var feedbackMessage = errorReport.ReportErrorType switch
                {
                    ReportErrorType.FormatError => await GetFeedbackMessageContent(SmsContentKey.ReportError.FormatError, errorReport.LanguageCode),
                    ReportErrorType.HealthRiskNotFound => await GetFeedbackMessageContent(SmsContentKey.ReportError.HealthRiskNotFound, errorReport.LanguageCode),
                    ReportErrorType.DataCollectorNotFound => null,
                    ReportErrorType.TooLong => null,
                    _ => await GetFeedbackMessageContent(SmsContentKey.ReportError.Other, errorReport.LanguageCode)
                };

                if (string.IsNullOrEmpty(feedbackMessage))
                {
                    _loggerAdapter.Warn($"No feedback message found for error type {errorReport.ReportErrorType}");
                    return;
                }

                var senderList = new List<SendSmsRecipient>
                {
                    new SendSmsRecipient
                    {
                        PhoneNumber = errorReport.Sender,
                        Modem = errorReport.ModemNumber
                    }
                };
                await _queuePublisherService.SendSms(senderList, gatewaySetting, feedbackMessage);
            }
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
