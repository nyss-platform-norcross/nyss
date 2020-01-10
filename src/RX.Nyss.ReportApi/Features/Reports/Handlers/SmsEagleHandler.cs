using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Features.Alerts;
using RX.Nyss.ReportApi.Features.Reports.Exceptions;
using RX.Nyss.ReportApi.Features.Reports.Models;
using RX.Nyss.ReportApi.Services;
using RX.Nyss.ReportApi.Utils;
using RX.Nyss.ReportApi.Utils.Logging;

namespace RX.Nyss.ReportApi.Features.Reports.Handlers
{
    public interface ISmsEagleHandler
    {
        Task Handle(string queryString);
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
        private readonly IEmailToSmsPublisherService _emailToSmsPublisherService;
        private readonly IAlertService _alertService;

        public SmsEagleHandler(
            IReportMessageService reportMessageService,
            INyssContext nyssContext,
            ILoggerAdapter loggerAdapter,
            IDateTimeProvider dateTimeProvider,
            IEmailToSmsPublisherService emailToSmsPublisherService, IAlertService alertService)
        {
            _reportMessageService = reportMessageService;
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
            _dateTimeProvider = dateTimeProvider;
            _emailToSmsPublisherService = emailToSmsPublisherService;
            _alertService = alertService;
        }

        public async Task Handle(string queryString)
        {
            var parsedQueryString = HttpUtility.ParseQueryString(queryString);
            var sender = parsedQueryString[SenderParameterName];
            var timestamp = parsedQueryString[TimestampParameterName];
            var text = parsedQueryString[TextParameterName];
            var incomingMessageId = parsedQueryString[IncomingMessageIdParameterName].ParseToNullableInt();
            var outgoingMessageId = parsedQueryString[OutgoingMessageIdParameterName].ParseToNullableInt();
            var modemNumber = parsedQueryString[ModemNumberParameterName].ParseToNullableInt();
            var apiKey = parsedQueryString[ApiKeyParameterName];

            try
            {
                Alert triggeredAlert = null;
                ProjectHealthRisk projectHealthRisk = null;
                GatewaySetting gatewaySetting = null;

                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var rawReport = new RawReport
                    {
                        Sender = sender,
                        Timestamp = timestamp,
                        ReceivedAt = _dateTimeProvider.UtcNow,
                        Text = text,
                        IncomingMessageId = incomingMessageId,
                        OutgoingMessageId = outgoingMessageId,
                        ModemNumber = modemNumber,
                        ApiKey = apiKey
                    };
                    await _nyssContext.AddAsync(rawReport);

                    var reportData = await ParseAndValidateReport(rawReport, parsedQueryString);
                    if (reportData != null)
                    {
                        gatewaySetting = reportData.GatewaySetting;
                        projectHealthRisk = reportData.ProjectHealthRisk;

                        var report = new Report
                        {
                            IsTraining = reportData.DataCollector.IsInTrainingMode,
                            ReportType = reportData.ParsedReport.ReportType,
                            Status = ReportStatus.New,
                            ReceivedAt = reportData.ReceivedAt,
                            CreatedAt = _dateTimeProvider.UtcNow,
                            DataCollector = reportData.DataCollector,
                            EpiWeek = _dateTimeProvider.GetEpiWeek(reportData.ReceivedAt),
                            PhoneNumber = sender,
                            Location = reportData.DataCollector.Location,
                            ReportedCase = reportData.ParsedReport.ReportedCase,
                            KeptCase = new ReportCase
                            {
                                CountMalesBelowFive = null,
                                CountMalesAtLeastFive = null,
                                CountFemalesBelowFive = null,
                                CountFemalesAtLeastFive = null
                            },
                            DataCollectionPointCase = reportData.ParsedReport.DataCollectionPointCase,
                            ProjectHealthRisk = projectHealthRisk,
                            Village = reportData.DataCollector.Village,
                            Zone = reportData.DataCollector.Zone,
                            ReportedCaseCount = projectHealthRisk.HealthRisk.HealthRiskType == HealthRiskType.Human
                                ? (reportData.ParsedReport.ReportedCase.CountFemalesAtLeastFive ?? 0)
                                    + (reportData.ParsedReport.ReportedCase.CountFemalesBelowFive ?? 0)
                                    + (reportData.ParsedReport.ReportedCase.CountMalesAtLeastFive ?? 0)
                                    + (reportData.ParsedReport.ReportedCase.CountMalesBelowFive ?? 0)
                                : 1
                        };

                        rawReport.Report = report;
                        await _nyssContext.Reports.AddAsync(report);
                        triggeredAlert = await _alertService.ReportAdded(report);
                    }

                    await _nyssContext.SaveChangesAsync();
                    transactionScope.Complete();
                }

                if (!string.IsNullOrEmpty(gatewaySetting?.EmailAddress) && projectHealthRisk != null)
                {
                    var recipients = new List<string>{ sender };
                    await _emailToSmsPublisherService.SendMessages(gatewaySetting.EmailAddress, gatewaySetting.Name, recipients, projectHealthRisk.FeedbackMessage);
                }

                if (triggeredAlert != null)
                {
                    await _alertService.SendNotificationsForNewAlert(triggeredAlert, gatewaySetting);
                }
            }
            catch (ReportValidationException e)
            {
                _loggerAdapter.Warn(e.Message);
            }
        }

        private async Task<ReportData> ParseAndValidateReport(RawReport rawReport, NameValueCollection parsedQueryString)
        {
            try
            {
                var apiKey = parsedQueryString[ApiKeyParameterName];
                var sender = parsedQueryString[SenderParameterName];
                var timestamp = parsedQueryString[TimestampParameterName];
                var text = parsedQueryString[TextParameterName];

                var gatewaySetting = await ValidateGatewaySetting(apiKey);
                rawReport.NationalSociety = gatewaySetting.NationalSociety;

                var dataCollector = await ValidateDataCollector(sender, gatewaySetting.NationalSociety);
                rawReport.DataCollector = dataCollector;
                rawReport.IsTraining = dataCollector.IsInTrainingMode;

                var parsedReport = _reportMessageService.ParseReport(text);
                var projectHealthRisk = await ValidateReport(parsedReport, dataCollector);

                var receivedAt = ParseTimestamp(timestamp);
                ValidateReceivalTime(receivedAt);
                rawReport.ReceivedAt = receivedAt;

                return new ReportData
                {
                    DataCollector = dataCollector,
                    ProjectHealthRisk = projectHealthRisk,
                    ReceivedAt = receivedAt,
                    GatewaySetting = gatewaySetting,
                    ParsedReport = parsedReport
                };
            }
            catch (Exception e)
            {
                _loggerAdapter.Warn(e.Message);
                return null;
            }
        }

        private async Task<GatewaySetting> ValidateGatewaySetting(string apiKey)
        {
            var gatewaySetting = await _nyssContext.GatewaySettings
                .Include(gs => gs.NationalSociety)
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

        private async Task<DataCollector> ValidateDataCollector(string phoneNumber, NationalSociety gatewayNationalSociety)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ReportValidationException("A phone number cannot be empty.");
            }

            var dataCollector = await _nyssContext.DataCollectors
                .Include(dc => dc.Project)
                .Include(dc => dc.Village)
                .Include(dc => dc.Zone)
                .SingleOrDefaultAsync(dc => dc.PhoneNumber == phoneNumber ||
                                            (dc.AdditionalPhoneNumber != null && dc.AdditionalPhoneNumber == phoneNumber));

            if (dataCollector == null)
            {
                throw new ReportValidationException($"A Data Collector with phone number '{phoneNumber}' does not exist.");
            }

            if (dataCollector.Project.NationalSocietyId != gatewayNationalSociety.Id)
            {
                throw new ReportValidationException($"A Data Collector's National Society identifier ('{dataCollector.Project.NationalSocietyId}') " +
                                                    $"is different from SMS Gateway's ('{gatewayNationalSociety.Id}').");
            }

            return dataCollector;
        }

        private async Task<ProjectHealthRisk> ValidateReport(ParsedReport parsedReport, DataCollector dataCollector)
        {
            switch (dataCollector.DataCollectorType)
            {
                case DataCollectorType.Human:
                    if (parsedReport.ReportType != ReportType.Single &&
                        parsedReport.ReportType != ReportType.Aggregate &&
                        parsedReport.ReportType != ReportType.NonHuman &&
                        parsedReport.ReportType != ReportType.Activity)
                    {
                        throw new ReportValidationException($"A data collector of type '{DataCollectorType.Human}' can only send a report of type " +
                                                            $"'{ReportType.Single}', '{ReportType.Aggregate}', '{ReportType.NonHuman}', '{ReportType.Activity}'.");
                    }

                    break;
                case DataCollectorType.CollectionPoint:
                    if (parsedReport.ReportType != ReportType.DataCollectionPoint &&
                        parsedReport.ReportType != ReportType.NonHuman &&
                        parsedReport.ReportType != ReportType.Activity)
                    {
                        throw new ReportValidationException($"A data collector of type '{DataCollectorType.CollectionPoint}' can only send a report of type " +
                                                            $"'{ReportType.DataCollectionPoint}', '{ReportType.NonHuman}', '{ReportType.Activity}'.");
                    }

                    break;
                default:
                    throw new ReportValidationException($"A data collector of type '{dataCollector.DataCollectorType}' is not supported.");
            }

            var projectHealthRisk = await _nyssContext.ProjectHealthRisks
                .Include(phr => phr.HealthRisk)
                .SingleOrDefaultAsync(phr => phr.HealthRisk.HealthRiskCode == parsedReport.HealthRiskCode &&
                                             phr.Project.Id == dataCollector.Project.Id);

            if (projectHealthRisk == null)
            {
                throw new ReportValidationException($"A health risk with code '{parsedReport.HealthRiskCode}' is not listed in project with id '{dataCollector.Project.Id}'.");
            }

            switch (parsedReport.ReportType)
            {
                case ReportType.Single:
                    if (projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Human)
                    {
                        throw new ReportValidationException($"A report of type '{ReportType.Single}' has to be related to '{HealthRiskType.Human}' health risk only.");
                    }

                    break;
                case ReportType.Aggregate:
                    if (projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Human)
                    {
                        throw new ReportValidationException($"A report of type '{ReportType.Aggregate}' has to be related to '{HealthRiskType.Human}' health risk only.");
                    }

                    break;
                case ReportType.NonHuman:
                    if (projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.NonHuman &&
                        projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.UnusualEvent)
                    {
                        throw new ReportValidationException(
                            $"A report of type '{ReportType.NonHuman}' has to be related to '{HealthRiskType.NonHuman}' or '{HealthRiskType.UnusualEvent}' event only.");
                    }

                    break;
                case ReportType.Activity:
                    if (projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity)
                    {
                        throw new ReportValidationException($"A report of type '{ReportType.Activity}' has to be related to '{HealthRiskType.Activity}' event only.");
                    }

                    break;
                case ReportType.DataCollectionPoint:
                    if (projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Human &&
                        projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.NonHuman &&
                        projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.UnusualEvent &&
                        projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity)
                    {
                        throw new ReportValidationException(
                            $"A report of type '{ReportType.DataCollectionPoint}' has to be related to '{HealthRiskType.Human}', '{HealthRiskType.NonHuman}', " +
                            $"'{HealthRiskType.UnusualEvent}', '{HealthRiskType.Activity}' event only.");
                    }

                    break;
                default:
                    throw new ReportValidationException($"A report of type '{parsedReport.ReportType}' is not supported.");
            }

            return projectHealthRisk;
        }

        private DateTime ParseTimestamp(string timestamp)
        {
            try
            {
                var formatProvider = CultureInfo.InvariantCulture;
                const string timestampFormat = "yyyyMMddHHmmss";

                var parsedSuccessfully = DateTime.TryParseExact(timestamp, timestampFormat, formatProvider, DateTimeStyles.None, out var parsedTimestamp);

                if (!parsedSuccessfully)
                {
                    throw new ReportValidationException($"Cannot parse timestamp '{timestamp}' to datetime.");
                }

                var parsedTimestampInUtc = DateTime.SpecifyKind(parsedTimestamp, DateTimeKind.Utc);

                return parsedTimestampInUtc;
            }
            catch (Exception e)
            {
                throw new ReportValidationException($"Cannot parse timestamp '{timestamp}'. Exception: {e.Message} Stack trace: {e.StackTrace}");
            }
        }

        private void ValidateReceivalTime(DateTime receivedAt)
        {
            const int maxAllowedPrecedenceInMinutes = 3;

            if (receivedAt > _dateTimeProvider.UtcNow.AddMinutes(maxAllowedPrecedenceInMinutes))
            {
                throw new ReportValidationException("The receival time cannot be in the future.");
            }
        }
    }
}
