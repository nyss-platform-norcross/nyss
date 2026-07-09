using System;
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

namespace RX.Nyss.ReportApi.Features.Reports.Handlers;

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
    private const string UtcOffsetParameterName = "utcoffset";
    private const string ApiKeyParameterName = "apikey";
    private const string FallbackLanguageCode = "en";

    private readonly INyssContext _nyssContext;
    private readonly ILoggerAdapter _loggerAdapter;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IReportMessageService _reportMessageService;
    private readonly IReportValidationService _reportValidationService;
    private readonly IAlertService _alertService;
    private readonly IStringsResourcesService _stringsResourcesService;
    private readonly IQueuePublisherService _queuePublisherService;
    private readonly IAlertNotificationService _alertNotificationService;

    public NyssReportHandler(
        INyssContext nyssContext,
        ILoggerAdapter loggerAdapter,
        IDateTimeProvider dateTimeProvider,
        IReportMessageService reportMessageService,
        IReportValidationService reportValidationService,
        IAlertService alertService,
        IStringsResourcesService stringsResourcesService,
        IQueuePublisherService queuePublisherService,
        IAlertNotificationService alertNotificationService)
    {
        _nyssContext = nyssContext;
        _loggerAdapter = loggerAdapter;
        _dateTimeProvider = dateTimeProvider;
        _reportMessageService = reportMessageService;
        _reportValidationService = reportValidationService;
        _alertService = alertService;
        _stringsResourcesService = stringsResourcesService;
        _queuePublisherService = queuePublisherService;
        _alertNotificationService = alertNotificationService;
    }

    public async Task Handle(string queryString)
    {
        var parsedQueryString = HttpUtility.ParseQueryString(queryString);

        var timestamp = parsedQueryString[TimestampParameterName];
        var text = parsedQueryString[TextParameterName]?.Trim() ?? string.Empty;
        var sentByHeadSupervisor = parsedQueryString[SentByHeadSupervisorSourceParameterName] == "true";
        var modemNumber = parsedQueryString[ModemNumberParameterName].ParseToNullableInt();
        var utcOffset = parsedQueryString[UtcOffsetParameterName].ParseToNullableInt();
        var apiKey = parsedQueryString[ApiKeyParameterName];

        DataCollector dataCollector = null;
        Report report = null;
        AlertData alertData = null;
        GatewaySetting gatewaySetting = null;
        ErrorReportData reportErrorData = null;

        using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
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
                var validationResult = await ParseAndValidateReport(rawReport, parsedQueryString);

                if (validationResult.IsSuccess)
                {
                    dataCollector = validationResult.ReportData.DataCollector;
                    gatewaySetting = validationResult.GatewaySetting;

                    report = CreateReport(rawReport, validationResult);
                    rawReport.Report = report;

                    await _nyssContext.Reports.AddAsync(report);
                    alertData = await _alertService.ReportAdded(report);
                }
                else
                {
                    rawReport.ErrorType = validationResult.ErrorReportData.ReportErrorType;
                    reportErrorData = validationResult.ErrorReportData;
                }
            }
            catch (ReportValidationException e)
            {
                _loggerAdapter.Warn(e.Message);
                rawReport.ErrorType = e.ErrorType;
                reportErrorData = new ErrorReportData { ReportErrorType = e.ErrorType };
            }
            catch (Exception e)
            {
                _loggerAdapter.Error(e, "Unexpected error while processing report");
                rawReport.ErrorType = ReportErrorType.Other;
                reportErrorData = new ErrorReportData { ReportErrorType = ReportErrorType.Other };
            }

            try
            {
                await _nyssContext.SaveChangesAsync();
                transactionScope.Complete();
            }
            catch (Exception e)
            {
                _loggerAdapter.Error(e, "Failed to save report to database");
                throw;
            }
        }

        if (reportErrorData != null)
        {
            return;
        }

        if (dataCollector == null || gatewaySetting == null)
        {
            _loggerAdapter.Warn("Skipping post-processing: dataCollector or gatewaySetting is null");
            return;
        }

        try
        {
            await HandleFeedbackMessageToDataCollector(dataCollector, gatewaySetting, report, modemNumber, sentByHeadSupervisor, utcOffset);
        }
        catch (Exception e)
        {
            _loggerAdapter.Error(e, "Failed to send feedback message to data collector");
        }

        try
        {
            await HandleAlertNotifications(alertData, gatewaySetting);
        }
        catch (Exception e)
        {
            _loggerAdapter.Error(e, "Failed to send alert notifications");
        }
    }

    private Report CreateReport(RawReport rawReport, ReportValidationResult validationResult)
    {
        var dataCollector = validationResult.ReportData.DataCollector;
        var gatewaySetting = validationResult.GatewaySetting;
        var parsedReport = validationResult.ReportData.ParsedReport;
        var projectHealthRisk = validationResult.ReportData.ProjectHealthRisk;

        var epiDate = _dateTimeProvider.GetEpiDate(
            rawReport.ReceivedAt,
            gatewaySetting.NationalSociety.EpiWeekStartDay);

        var hasSingleLocation = dataCollector.DataCollectorLocations?.Count == 1;

        return new Report
        {
            IsTraining = dataCollector.IsInTrainingMode,
            ReportType = parsedReport.ReportType,
            Status = ReportStatus.New,
            ReceivedAt = validationResult.ReportData.ReceivedAt,
            CreatedAt = _dateTimeProvider.UtcNow,
            DataCollector = dataCollector,
            EpiWeek = epiDate.EpiWeek,
            EpiYear = epiDate.EpiYear,
            PhoneNumber = dataCollector.PhoneNumber,
            Location = hasSingleLocation
                ? dataCollector.DataCollectorLocations.First().Location
                : null,
            ReportedCase = parsedReport.ReportedCase,
            DataCollectionPointCase = parsedReport.DataCollectionPointCase,
            ProjectHealthRisk = projectHealthRisk,
            ReportedCaseCount = CalculateReportedCaseCount(parsedReport)
        };
    }

    private static int CalculateReportedCaseCount(ParsedReport parsedReport)
    {
        if (parsedReport.ReportType == ReportType.Event)
        {
            return 1;
        }

        var reportedCase = parsedReport.ReportedCase;
        if (reportedCase == null)
        {
            return 0;
        }

        return (reportedCase.CountFemalesAtLeastFive ?? 0)
            + (reportedCase.CountFemalesBelowFive ?? 0)
            + (reportedCase.CountMalesAtLeastFive ?? 0)
            + (reportedCase.CountMalesBelowFive ?? 0)
            + (reportedCase.CountUnspecifiedSexAndAge ?? 0);
    }

    private async Task<ReportValidationResult> ParseAndValidateReport(RawReport rawReport, NameValueCollection parsedQueryString)
    {
        try
        {
            if (!int.TryParse(parsedQueryString[DataCollectorIdParameterName], out var dataCollectorId))
            {
                throw new ReportValidationException("Invalid or missing data collector ID.", ReportErrorType.Other);
            }

            var timestamp = parsedQueryString[TimestampParameterName];
            var text = parsedQueryString[TextParameterName]?.Trim() ?? string.Empty;
            var modemNumber = parsedQueryString[ModemNumberParameterName].ParseToNullableInt();
            var apiKey = parsedQueryString[ApiKeyParameterName];

            var receivedAt = _reportValidationService.ParseTimestamp(timestamp);
            _reportValidationService.ValidateReceivalTime(receivedAt);
            rawReport.ReceivedAt = receivedAt;

            var gatewaySetting = await _reportValidationService.ValidateGatewaySetting(apiKey);
            var dataCollector = await ValidateDataCollector(dataCollectorId, gatewaySetting.NationalSocietyId);

            var hasSingleLocation = dataCollector?.DataCollectorLocations?.Count == 1;
            rawReport.DataCollector = dataCollector;
            rawReport.NationalSociety = dataCollector?.Project.NationalSociety ?? gatewaySetting.NationalSociety;
            rawReport.IsTraining = dataCollector?.IsInTrainingMode ?? false;
            rawReport.Village = hasSingleLocation ? dataCollector.DataCollectorLocations.First().Village : null;
            rawReport.Zone = hasSingleLocation ? dataCollector.DataCollectorLocations.First().Zone : null;

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
                    ModemNumber = modemNumber,
                    ReceivedAt = receivedAt
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

    private async Task<DataCollector> ValidateDataCollector(int dataCollectorId, int nationalSocietyId)
    {
        var dataCollector = await _nyssContext.DataCollectors
            .Include(dc => dc.Supervisor)
                .ThenInclude(s => s.HeadSupervisor)
            .Include(dc => dc.HeadSupervisor)
            .Include(dc => dc.Project)
                .ThenInclude(p => p.NationalSociety)
            .Include(dc => dc.DataCollectorLocations)
                .ThenInclude(dcl => dcl.Village)
            .Include(dc => dc.DataCollectorLocations)
                .ThenInclude(dcl => dcl.Zone)
            .Where(dc => dc.Id == dataCollectorId)
            .SingleOrDefaultAsync();

        if (dataCollector == null)
        {
            throw new ReportValidationException($"Data collector with ID '{dataCollectorId}' was not found.", ReportErrorType.Other);
        }

        if (dataCollector.Project.NationalSocietyId != nationalSocietyId)
        {
            throw new ReportValidationException(
                $"Data collector's National Society ID ('{dataCollector.Project.NationalSocietyId}') " +
                $"does not match the SMS Gateway's National Society ID ('{nationalSocietyId}').",
                ReportErrorType.Other);
        }

        return dataCollector;
    }

    private async Task HandleFeedbackMessageToDataCollector(
        DataCollector dataCollector,
        GatewaySetting gatewaySetting,
        Report report,
        int? modemNumber,
        bool sentByHeadSupervisor,
        int? utcOffset)
    {
        if (string.IsNullOrEmpty(dataCollector?.PhoneNumber))
        {
            _loggerAdapter.Warn("Skipping feedback message: data collector phone number is missing");
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

        var nyssPrefix = await GetFeedbackMessage(report, dataCollector, gatewaySetting, sentByHeadSupervisor, utcOffset);
        var feedbackMessage = !string.IsNullOrEmpty(nyssPrefix)
            ? $"{nyssPrefix} {report.ProjectHealthRisk.FeedbackMessage}"
            : report.ProjectHealthRisk.FeedbackMessage;

        await _queuePublisherService.SendSms(recipients, gatewaySetting, feedbackMessage);
    }

    private async Task HandleAlertNotifications(AlertData alertData, GatewaySetting gatewaySetting)
    {
        if (alertData?.Alert == null)
        {
            return;
        }

        if (alertData.IsExistingAlert)
        {
            await _alertNotificationService.SendNotificationsForSupervisorsAddedToExistingAlert(
                alertData.Alert,
                alertData.SupervisorsAddedToExistingAlert,
                gatewaySetting);
        }
        else
        {
            await _alertNotificationService.SendNotificationsForNewAlert(alertData.Alert, gatewaySetting);
        }
    }

    private async Task<string> GetFeedbackMessage(
        Report report,
        DataCollector dataCollector,
        GatewaySetting gatewaySetting,
        bool sentByHeadSupervisor,
        int? utcOffset)
    {
        var languageCode = await _nyssContext.NationalSocieties
            .Where(ns => ns.Id == gatewaySetting.NationalSocietyId)
            .Select(ns => ns.ContentLanguage.LanguageCode)
            .FirstOrDefaultAsync() ?? FallbackLanguageCode;

        var feedbackMessage = await GetFeedbackMessageContent(SmsContentKey.Reports.ReportSentFromNyss, languageCode);
        if (string.IsNullOrEmpty(feedbackMessage))
        {
            return string.Empty;
        }

        var senderName = GetSenderName(dataCollector, sentByHeadSupervisor);

        var languageContents = await _nyssContext.HealthRiskLanguageContents
            .SingleOrDefaultAsync(hlc =>
                hlc.HealthRisk == report.ProjectHealthRisk.HealthRisk &&
                hlc.ContentLanguage.LanguageCode == languageCode);

        if (languageContents == null)
        {
            _loggerAdapter.Warn($"No health risk language content found for language '{languageCode}'");
        }

        var timestamp = report.ReceivedAt
            .AddHours(utcOffset ?? 0)
            .ToString("yyyy-MM-dd HH:mm");

        return feedbackMessage
            .Replace("{{supervisor}}", senderName)
            .Replace("{{date/time}}", timestamp)
            .Replace("{{health risk/event}}", languageContents?.Name ?? string.Empty);
    }

    private string GetSenderName(DataCollector dataCollector, bool sentByHeadSupervisor)
    {
        if (dataCollector.Supervisor != null)
        {
            return sentByHeadSupervisor && dataCollector.Supervisor.HeadSupervisor != null
                ? dataCollector.Supervisor.HeadSupervisor.Name
                : dataCollector.Supervisor.Name;
        }

        return dataCollector.HeadSupervisor?.Name ?? string.Empty;
    }

    private async Task<string> GetFeedbackMessageContent(string key, string languageCode)
    {
        var effectiveLanguageCode = !string.IsNullOrEmpty(languageCode)
            ? languageCode
            : FallbackLanguageCode;

        var smsContents = await _stringsResourcesService.GetSmsContentResources(effectiveLanguageCode);
        smsContents.Value.TryGetValue(key, out var message);

        if (message == null)
        {
            _loggerAdapter.Warn($"No SMS content resource found for key '{key}' with language '{effectiveLanguageCode}'");
        }

        return message;
    }
}