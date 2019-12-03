using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Exceptions;
using RX.Nyss.ReportApi.Models;
using RX.Nyss.ReportApi.Services;
using RX.Nyss.ReportApi.Utils;
using RX.Nyss.ReportApi.Utils.Logging;

namespace RX.Nyss.ReportApi.Handlers
{
    public class SmsEagleHandler : ISmsHandler
    {
        private const string SenderParameterName = "sender";
        private const string TimestampParameterName = "timestamp";
        private const string TextParameterName = "text";
        private const string IncomingMessageIdParameterName = "msgid";
        private const string OutgoingMessageIdParameterName = "oid";
        private const string ModemNumberParameterName = "modemno";
        private const string ApiKeyParameterName = "apikey";

        private static readonly string[] RequiredQueryStringParameters =
        {
            SenderParameterName,
            TimestampParameterName,
            TextParameterName,
            IncomingMessageIdParameterName,
            OutgoingMessageIdParameterName,
            ModemNumberParameterName,
            ApiKeyParameterName
        };

        private readonly IReportMessageService _reportMessageService;
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEmailToSMSPublisherService _emailToSMSPublisherService;

        public SmsEagleHandler(IReportMessageService reportMessageService, INyssContext nyssContext, ILoggerAdapter loggerAdapter, IDateTimeProvider dateTimeProvider, IEmailToSMSPublisherService emailToSMSPublisherService)
        {
            _reportMessageService = reportMessageService;
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
            _dateTimeProvider = dateTimeProvider;
            _emailToSMSPublisherService = emailToSMSPublisherService;
        }

        public bool CanHandle(string queryString)
        {
            var parsedQueryString = HttpUtility.ParseQueryString(queryString);
            return RequiredQueryStringParameters.All(parsedQueryString.AllKeys.Contains);
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
            await _nyssContext.SaveChangesAsync();

            //ToDo: extract try-catch block to a separate service?
            try
            {
                var gatewaySetting = await ValidateGatewaySetting(apiKey);
                rawReport.NationalSociety = gatewaySetting.NationalSociety;
                await _nyssContext.SaveChangesAsync();

                var dataCollector = await ValidateDataCollector(sender, gatewaySetting.NationalSociety);
                rawReport.DataCollector = dataCollector;
                await _nyssContext.SaveChangesAsync();

                var parsedReport = _reportMessageService.ParseReport(text);
                var projectHealthRisk = await ValidateReport(parsedReport, dataCollector);

                var receivedAt = ParseTimestamp(timestamp, dataCollector.Project.TimeZone);
                ValidateReceiptTime(receivedAt);
                rawReport.ReceivedAt = receivedAt;
                await _nyssContext.SaveChangesAsync();

                var report = new Report
                {
                    IsTraining = dataCollector.IsInTrainingMode,
                    ReportType = parsedReport.ReportType,
                    Status = ReportStatus.Pending,
                    ReceivedAt = receivedAt,
                    CreatedAt = _dateTimeProvider.UtcNow,
                    DataCollector = dataCollector,
                    EpiWeek = _dateTimeProvider.GetEpiWeek(receivedAt),
                    PhoneNumber = sender,
                    Location = dataCollector.Location,
                    ReportedCase = parsedReport.ReportedCase,
                    KeptCase = new ReportCase
                    {
                        CountMalesBelowFive = null,
                        CountMalesAtLeastFive = null,
                        CountFemalesBelowFive = null,
                        CountFemalesAtLeastFive = null
                    },
                    DataCollectionPointCase = parsedReport.DataCollectionPointCase,
                    ProjectHealthRisk = projectHealthRisk,
                    Village = dataCollector.Village,
                    Zone = dataCollector.Zone
                };

                rawReport.Report = report;

                await _nyssContext.Reports.AddAsync(report);
                await _nyssContext.SaveChangesAsync();

                if (!string.IsNullOrEmpty(gatewaySetting.EmailAddress))
                {
                    var recipients = new List<string>{ sender };
                    await _emailToSMSPublisherService.SendMessage(gatewaySetting.EmailAddress, gatewaySetting.Name, recipients, projectHealthRisk.FeedbackMessage);
                }
            }
            catch (ReportValidationException e)
            {
                _loggerAdapter.Warn(e.Message);
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
            var dataCollector = await _nyssContext.DataCollectors
                .Include(dc => dc.Project)
                .SingleOrDefaultAsync(dc => dc.PhoneNumber == phoneNumber || dc.AdditionalPhoneNumber == phoneNumber);

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
            if (dataCollector.DataCollectorType != parsedReport.DataCollectorType)
            {
                throw new ReportValidationException($"A detected Data Collector type ('{parsedReport.DataCollectorType}') is different than the real Data Collector type {dataCollector.DataCollectorType}.");
            }

            var projectHealthRisk = await _nyssContext.ProjectHealthRisks
                .Include(phr => phr.HealthRisk)
                .SingleOrDefaultAsync(phr => phr.HealthRisk.HealthRiskCode == parsedReport.HealthRiskCode &&
                                             phr.Project.Id == dataCollector.Project.Id);

            if (projectHealthRisk == null)
            {
                throw new ReportValidationException($"A health risk with code '{parsedReport.HealthRiskCode}' is not listed in project with id '{dataCollector.Project.Id}'.");
            }

            if (parsedReport.ReportType == ReportType.Aggregate &&
                parsedReport.ReportedCase.CountMalesBelowFive == 0 &&
                parsedReport.ReportedCase.CountMalesAtLeastFive == 0 &&
                parsedReport.ReportedCase.CountFemalesBelowFive == 0 &&
                parsedReport.ReportedCase.CountFemalesAtLeastFive == 0)
            {
                throw new ReportValidationException("At least one number in aggregated report must be greater than 0 " +
                                    $"(males below five: {parsedReport.ReportedCase.CountMalesBelowFive}, " +
                                    $"males at least five: {parsedReport.ReportedCase.CountMalesAtLeastFive}, " +
                                    $"females below five: {parsedReport.ReportedCase.CountFemalesBelowFive}, " +
                                    $"females at least five: {parsedReport.ReportedCase.CountFemalesAtLeastFive}).");
            }

            switch (parsedReport.ReportType)
            {
                case ReportType.Single when projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Human:
                    throw new ReportValidationException($"A report of type '{ReportType.Single}' has to be related to '{HealthRiskType.Human}' health risk only.");
                case ReportType.Aggregate when projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Human:
                    throw new ReportValidationException($"A report of type '{ReportType.Aggregate}' has to be related to '{HealthRiskType.Human}' health risk only.");
                case ReportType.NonHuman when projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.NonHuman &&
                                              projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.UnusualEvent:
                    throw new ReportValidationException(
                        $"A report of type '{ReportType.NonHuman}' has to be related to '{HealthRiskType.NonHuman}' or '{HealthRiskType.UnusualEvent}' event only.");
                case ReportType.Activity when projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity:
                    throw new ReportValidationException($"A report of type '{ReportType.Activity}' has to be related to '{HealthRiskType.Activity}' event only.");
                case ReportType.DataCollectionPoint:
                    //ToDo: implement DataCollectionPoint logic
                    break;
                default:
                    break;
            }

            return projectHealthRisk;
        }

        private DateTime ParseTimestamp(string timestamp, string timeZoneName)
        {
            try
            {
                var formatProvider = CultureInfo.InvariantCulture;
                const string timestampFormat = "yyyyMMddHHmmss";

                var timeZone = _dateTimeProvider.GetTimeZoneInfo(timeZoneName);

                var parsedSuccessfully = DateTime.TryParseExact(timestamp, timestampFormat, formatProvider, DateTimeStyles.None, out var dateTime);

                if (!parsedSuccessfully)
                {
                    throw new ReportValidationException($"Cannot parse timestamp '{timestamp}' to datetime.");
                }

                var parsedTimestampInUtc = TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone);

                return parsedTimestampInUtc;
            }
            catch (Exception e)
            {
                throw new ReportValidationException($"Cannot parse timestamp '{timestamp}' for time zone '{timeZoneName}'. Exception: {e.Message} Stack trace: {e.StackTrace}");
            }
        }

        private void ValidateReceiptTime(DateTime receivedAt)
        {
            const int maxAllowedPrecedenceInMinutes = 3;

            if (receivedAt > _dateTimeProvider.UtcNow.AddMinutes(maxAllowedPrecedenceInMinutes))
            {
                throw new ReportValidationException("The receipt time cannot be in the future.");
            }
        }
    }
}
