using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Features.Alerts;
using RX.Nyss.ReportApi.Features.Common.Extensions;
using RX.Nyss.ReportApi.Features.Reports.Contracts;
using RX.Nyss.ReportApi.Features.Reports.Exceptions;
using RX.Nyss.ReportApi.Features.Reports.Models;
using RX.Nyss.ReportApi.Services;
using Report = RX.Nyss.Data.Models.Report;

namespace RX.Nyss.ReportApi.Features.Reports.Handlers;

public interface ISmsGatewayMTNHandler
{
    Task Handle(string queryString);

    Task<DataCollector> ValidateDataCollector(string phoneNumber, int gatewayNationalSocietyId);
}

public class SmsGatewayMTNHandler : ISmsGatewayMTNHandler
{
    private readonly IReportMessageService _reportMessageService;

    private readonly INyssContext _nyssContext;

    private readonly ILoggerAdapter _loggerAdapter;

    private readonly IDateTimeProvider _dateTimeProvider;

    private readonly IQueuePublisherService _queuePublisherService;

    private readonly IAlertService _alertService;

    private readonly IStringsResourcesService _stringsResourcesService;

    private readonly IReportValidationService _reportValidationService;

    private readonly IAlertNotificationService _alertNotificationService;

    public SmsGatewayMTNHandler(
        IReportMessageService reportMessageService,
        INyssContext nyssContext,
        ILoggerAdapter loggerAdapter,
        IDateTimeProvider dateTimeProvider,
        IStringsResourcesService stringsResourcesService,
        IQueuePublisherService queuePublisherService,
        IAlertService alertService,
        IReportValidationService reportValidationService,
        IAlertNotificationService alertNotificationService)
    {
        _reportMessageService = reportMessageService;
        _nyssContext = nyssContext;
        _loggerAdapter = loggerAdapter;
        _dateTimeProvider = dateTimeProvider;
        _queuePublisherService = queuePublisherService;
        _alertService = alertService;
        _stringsResourcesService = stringsResourcesService;
        _reportValidationService = reportValidationService;
        _alertNotificationService = alertNotificationService;
    }

    public async Task Handle(string queryString)
    {
        var decodedQueryString = HttpUtility.UrlDecode(queryString);
        var mtnReportObject = JsonConvert.DeserializeObject<MTNReport>(decodedQueryString);

        var senderAddress = mtnReportObject.SenderAddress;
        if (mtnReportObject.SenderAddress != null)
        {
            senderAddress = mtnReportObject.SenderAddress.StartsWith("+")
                ? mtnReportObject.SenderAddress
                : string.Concat("+", mtnReportObject.SenderAddress);
        }

        ErrorReportData errorReportData = null;
        AlertData alertData = null;
        ProjectHealthRisk projectHealthRisk = null;
        GatewaySetting gatewaySetting;

        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var rawReport = new RawReport
            {
                Sender = senderAddress,
                Timestamp = UnixTimeStampToDateTime((long)Convert.ToDouble(mtnReportObject.Created)).ToString("yyyyMMddHHmmss"),
                ReceivedAt = _dateTimeProvider.UtcNow,
                Text = mtnReportObject.Message,
                ApiKey = mtnReportObject.ReceiverAddress,//We haven't apikey but we will keep short code value for using as a reference later
            };
            await _nyssContext.AddAsync(rawReport);

            mtnReportObject.SenderAddress = senderAddress;// Just for taking care of + at start of phone number
            var reportValidationResult = await ParseAndValidateReport(rawReport, mtnReportObject);
            if (reportValidationResult.IsSuccess)
            {
                gatewaySetting = reportValidationResult.GatewaySetting;
                projectHealthRisk = reportValidationResult.ReportData.ProjectHealthRisk;
                var reportData = reportValidationResult.ReportData;

                var epiDate = _dateTimeProvider.GetEpiDate(reportValidationResult.ReportData.ReceivedAt, gatewaySetting.NationalSociety.EpiWeekStartDay);
                var phoneNumber = "";
                if (reportData.DataCollector != null)
                {
                    phoneNumber = reportData.DataCollector.PhoneNumber ?? "";
                }
                var report = new Report
                {
                    IsTraining = reportData.DataCollector?.IsInTrainingMode ?? false,
                    ReportType = reportData.ParsedReport.ReportType,
                    Status = ReportStatus.New,
                    ReceivedAt = reportData.ReceivedAt,
                    CreatedAt = _dateTimeProvider.UtcNow,
                    DataCollector = reportData.DataCollector,
                    EpiWeek = epiDate.EpiWeek,
                    EpiYear = epiDate.EpiYear,
                    PhoneNumber = phoneNumber,
                    Location = reportData.DataCollector?.DataCollectorLocations.Count == 1
                        ? reportData.DataCollector.DataCollectorLocations.First().Location
                        : null,
                    ReportedCase = reportData.ParsedReport.ReportedCase,
                    DataCollectionPointCase = reportData.ParsedReport.DataCollectionPointCase,
                    ProjectHealthRisk = projectHealthRisk,
                    ReportedCaseCount = projectHealthRisk.HealthRisk.HealthRiskType == HealthRiskType.Human
                        ? (reportData.ParsedReport.ReportedCase.CountFemalesAtLeastFive ?? 0)
                        + (reportData.ParsedReport.ReportedCase.CountFemalesBelowFive ?? 0)
                        + (reportData.ParsedReport.ReportedCase.CountMalesAtLeastFive ?? 0)
                        + (reportData.ParsedReport.ReportedCase.CountMalesBelowFive ?? 0)
                        + (reportData.ParsedReport.ReportedCase.CountUnspecifiedSexAndAge ?? 0)
                        : 1
                };

                rawReport.Report = report;
                await _nyssContext.Reports.AddAsync(report);
                alertData = await _alertService.ReportAdded(report);
            }
            else
            {
                errorReportData = reportValidationResult.ErrorReportData;
                gatewaySetting = reportValidationResult.GatewaySetting;
                rawReport.ErrorType = errorReportData.ReportErrorType;
            }

            await _nyssContext.SaveChangesAsync();
            transactionScope.Complete();
        }

        await SendNotifications(senderAddress, alertData, errorReportData, projectHealthRisk, gatewaySetting);
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
            .Include(dc => dc.HeadSupervisor)
            .Include(dc => dc.Project)
            .Include(dc => dc.DataCollectorLocations)
            .ThenInclude(dcl => dcl.Village)
            .Include(dc => dc.DataCollectorLocations)
            .ThenInclude(dcl => dcl.Zone)
            .SingleOrDefaultAsync(dc => dc.PhoneNumber == phoneNumber ||
                (dc.AdditionalPhoneNumber != null && dc.AdditionalPhoneNumber == phoneNumber));

        if (dataCollector != null && dataCollector.Project.NationalSocietyId != gatewayNationalSocietyId)
        {
            throw new ReportValidationException($"A Data Collector's National Society identifier ('{dataCollector.Project.NationalSocietyId}') " +
                $"is different from SMS Gateway's ('{gatewayNationalSocietyId}').");
        }

        return dataCollector;
    }

    private async Task<ReportValidationResult> ParseAndValidateReport(RawReport rawReport, MTNReport mtnReportObject)
    {
        GatewaySetting gatewaySetting = null;
        DataCollector dataCollector = null;
        try
        {
            gatewaySetting = await _reportValidationService.ValidateMTNGatewaySetting(mtnReportObject.ReceiverAddress);
            rawReport.NationalSociety = gatewaySetting.NationalSociety;
            _reportValidationService.ValidateReceivalTime(rawReport.ReceivedAt);
            dataCollector = await ValidateDataCollector(mtnReportObject.SenderAddress, gatewaySetting.NationalSocietyId);
            rawReport.DataCollector = dataCollector;
            rawReport.IsTraining = dataCollector?.IsInTrainingMode ?? false;
            rawReport.Village = dataCollector?.DataCollectorLocations.Count == 1
                ? dataCollector.DataCollectorLocations.First().Village
                : null;
            rawReport.Zone = dataCollector?.DataCollectorLocations.Count == 1
                ? dataCollector.DataCollectorLocations.First().Zone
                : null;

            var parsedReport = await _reportMessageService.ParseReport(mtnReportObject.Message);
            var projectHealthRisk = await _reportValidationService.ValidateReport(parsedReport, dataCollector, gatewaySetting.NationalSocietyId);

            return new ReportValidationResult
            {
                IsSuccess = true,
                ReportData = new ReportData
                {
                    DataCollector = dataCollector,
                    ProjectHealthRisk = projectHealthRisk,
                    ReceivedAt = rawReport.ReceivedAt,
                    ParsedReport = parsedReport
                },
                GatewaySetting = gatewaySetting
            };
        }
        catch (ReportValidationException e)
        {
            _loggerAdapter.Warn(e);

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
                    DataCollector = dataCollector,
                    LanguageCode = languageCode,
                    ReportErrorType = e.ErrorType
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

    private async Task SendNotifications(
        string senderPhoneNumber,
        AlertData alertData,
        ErrorReportData errorReportData,
        ProjectHealthRisk projectHealthRisk,
        GatewaySetting gatewaySetting)
    {
        if (errorReportData == null)
        {
            var recipients = new List<SendGatewaySmsRecipient>
            {
                new SendGatewaySmsRecipient
                {
                    PhoneNumber = senderPhoneNumber
                }
            };

            await _queuePublisherService.SendMTNGatewayHttpSms(recipients, gatewaySetting, projectHealthRisk.FeedbackMessage);

            if (alertData.Alert != null)
            {
                if (alertData.IsExistingAlert)
                {
                    await _alertNotificationService.SendNotificationsForSupervisorsAddedToExistingAlert(alertData.Alert, alertData.SupervisorsAddedToExistingAlert, gatewaySetting);
                }
                else
                {
                    await _alertNotificationService.SendNotificationsForNewAlert(alertData.Alert, gatewaySetting);
                }
            }
        }
        else
        {
            await SendFeedbackOnError(errorReportData, gatewaySetting);
        }
    }

    private async Task SendFeedbackOnError(ErrorReportData errorReport, GatewaySetting gatewaySetting)
    {
        if (gatewaySetting == null || errorReport.DataCollector == null)
        {
            return;
        }

        var smsErrorKey = errorReport.ReportErrorType.ToSmsErrorKey();

        var projectErrorMessage = await _nyssContext.ProjectErrorMessages
            .SingleOrDefaultAsync(x => x.ProjectId == errorReport.DataCollector.Project.Id && x.MessageKey == smsErrorKey);

        var feedbackMessage = projectErrorMessage?.Message ?? await GetFeedbackMessageContent(smsErrorKey, errorReport.LanguageCode);

        if (string.IsNullOrEmpty(feedbackMessage))
        {
            _loggerAdapter.Warn($"No feedback message found for error type {errorReport.ReportErrorType}");
            return;
        }

        var senderList = new List<SendGatewaySmsRecipient>
        {
            new SendGatewaySmsRecipient
            {
                PhoneNumber = errorReport.DataCollector.PhoneNumber
            }
        };

        await _queuePublisherService.SendGatewayHttpSms(senderList, gatewaySetting, feedbackMessage);
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

    public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        // the unixTimeStamp is comming from a java server so it will be based on milisecond so we need to convert it to second!
        var truncatedUnixTimeStamp = long.Parse(unixTimeStamp.ToString().Remove(unixTimeStamp.ToString().Length - 3));
        // Unix timestamp is seconds past epoch
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        try
        {
            dateTime = dateTime.AddSeconds(truncatedUnixTimeStamp).ToLocalTime();
        }
        catch
        { dateTime = DateTime.Now; }

        return dateTime;
    }
}

