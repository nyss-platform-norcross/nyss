using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
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
        private const string MessageIdParameterName = "msgid";
        private const string OutgoingMessageIdParameterName = "oid";
        private const string ModemNumberParameterName = "modemno";
        private const string ApiKeyParameterName = "apikey";
        
        private static readonly string[] RequiredQueryStringParameters =
        {
            SenderParameterName,
            TimestampParameterName,
            TextParameterName,
            MessageIdParameterName,
            OutgoingMessageIdParameterName,
            ModemNumberParameterName,
            ApiKeyParameterName
        };

        private readonly IReportMessageService _reportMessageService;
        private readonly INyssReportContext _nyssReportContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IDateTimeProvider _dateTimeProvider;

        public SmsEagleHandler(IReportMessageService reportMessageService, INyssReportContext nyssReportContext, ILoggerAdapter loggerAdapter, IDateTimeProvider dateTimeProvider)
        {
            _reportMessageService = reportMessageService;
            _nyssReportContext = nyssReportContext;
            _loggerAdapter = loggerAdapter;
            _dateTimeProvider = dateTimeProvider;
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
            var apiKey = parsedQueryString[ApiKeyParameterName];

            // ToDo: messageId, outgoingMessageId and modemNumber are not used
            var messageId = parsedQueryString[MessageIdParameterName].ParseToNullableInt();
            var outgoingMessageId = parsedQueryString[OutgoingMessageIdParameterName];
            var modemNumber = parsedQueryString[ModemNumberParameterName].ParseToNullableInt();
            
            var isValid = true;

            var (gatewayValidationResult, gatewayNationalSocietyId) = await ValidateGatewaySetting(apiKey);

            isValid &= gatewayValidationResult;

            var (dataCollectorValidationResult, dataCollector) = await ValidateDataCollector(sender, gatewayNationalSocietyId);

            isValid &= dataCollectorValidationResult;

            var (reportParsedSuccessfully, parsedReport) = _reportMessageService.ParseReport(text);

            var (reportValidationResult, projectHealthRisk) = await ValidateReport(reportParsedSuccessfully, parsedReport, sender, dataCollector);

            isValid &= reportValidationResult;

            var receivedAt = ParseTimestamp(timestamp, dataCollector?.Project?.TimeZone);

            if (receivedAt > _dateTimeProvider.UtcNow)
            {
                _loggerAdapter.Warn("The receipt time cannot be in the future.");
                isValid = false;
            }

            var report = new Report
            {
                RawContent = text,
                IsValid = isValid,
                IsTraining = dataCollector?.IsInTrainingMode,
                ReportType = parsedReport.ReportType,
                Status = ReportStatus.Pending,
                //ToDo: NULL vs current date
                ReceivedAt = receivedAt ?? _dateTimeProvider.UtcNow,
                CreatedAt = _dateTimeProvider.UtcNow,
                DataCollector = dataCollector,
                Location = dataCollector?.Location,
                ReportedCase = parsedReport.ReportedCase,
                KeptCase = new ReportCase
                {
                    CountMalesBelowFive = null,
                    CountMalesAtLeastFive = null,
                    CountFemalesBelowFive = null,
                    CountFemalesAtLeastFive = null
                },
                DataCollectionPointCase = parsedReport.DataCollectionPointCase,
                ProjectHealthRisk = projectHealthRisk
            };

            await _nyssReportContext.Reports.AddAsync(report);

            await _nyssReportContext.SaveChangesAsync();
        }

        private async Task<(bool gatewayValidationResult, int? gatewayNationalSocietyId)> ValidateGatewaySetting(string apiKey)
        {
            var isValid = true;

            var gatewaySetting = await _nyssReportContext.GatewaySettings
                .SingleOrDefaultAsync(gs => gs.ApiKey == apiKey);

            if (gatewaySetting == null)
            {
                _loggerAdapter.Warn($"A gateway setting with API key '{apiKey}' does not exist.");
                isValid = false;
            }

            if (gatewaySetting != null && gatewaySetting.GatewayType != GatewayType.SmsEagle)
            {
                _loggerAdapter.Warn($"A gateway type ('{gatewaySetting.GatewayType}') is different than '{GatewayType.SmsEagle}'.");
                isValid = false;
            }

            return (isValid, gatewaySetting?.NationalSocietyId);
        }

        private async Task<(bool dataCollectorValidationResult, DataCollector dataCollector)> ValidateDataCollector(string phoneNumber, int? gatewayNationalSocietyId)
        {
            var isValid = true;

            var dataCollector = await _nyssReportContext.DataCollectors
                .Include(dc => dc.Project)
                .SingleOrDefaultAsync(dc => dc.PhoneNumber == phoneNumber || dc.AdditionalPhoneNumber == phoneNumber);

            if (dataCollector == null)
            {
                _loggerAdapter.Warn($"A Data Collector with phone number '{phoneNumber}' does not exist.");
                isValid = false;
            }

            if (gatewayNationalSocietyId.HasValue && dataCollector != null && dataCollector.Project.NationalSocietyId != gatewayNationalSocietyId)
            {
                _loggerAdapter.Warn($"A Data Collector's National Society identifier ('{dataCollector.Project.NationalSocietyId}') " +
                                    $"is different than SMS Gateway's ('{gatewayNationalSocietyId}').");
                isValid = false;
            }

            return (isValid, dataCollector);
        }

        private async Task<(bool reportValidationResult, ProjectHealthRisk projectHealthRisk)> ValidateReport(bool reportParsedSuccessfully, ReportMessageService.ParsedReport parsedReport, string sender, DataCollector dataCollector)
        {
            var isValid = true;

            if (!reportParsedSuccessfully)
            {
                _loggerAdapter.Warn("A report cannot be parsed.");
                isValid = false;
            }

            if (reportParsedSuccessfully && dataCollector != null && dataCollector.DataCollectorType != parsedReport.DataCollectorType)
            {
                _loggerAdapter.Warn($"A detected Data Collector type ('{parsedReport.DataCollectorType}') is different than the real Data Collector type {dataCollector.DataCollectorType}.");
                isValid = false;
            }

            var projectHealthRisk = await _nyssReportContext.ProjectHealthRisks
                .Include(phr => phr.HealthRisk)
                .SingleOrDefaultAsync(phr => phr.HealthRisk.HealthRiskCode == parsedReport.HealthRiskCode &&
                                             phr.Project.Id == dataCollector.Project.Id);

            if (projectHealthRisk == null)
            {
                _loggerAdapter.Warn($"A health risk with code '{parsedReport.HealthRiskCode}' is not listed in project with id '{dataCollector.Project.Id}'.");
                isValid = false;
            }

            if (parsedReport.ReportType == ReportType.Aggregate &&
                parsedReport.ReportedCase.CountMalesBelowFive == 0 &&
                parsedReport.ReportedCase.CountMalesAtLeastFive == 0 &&
                parsedReport.ReportedCase.CountFemalesBelowFive == 0 &&
                parsedReport.ReportedCase.CountFemalesAtLeastFive == 0)
            {
                _loggerAdapter.Warn($"At least one number in aggregated report must be greater than 0 " +
                                    $"(males below five: {parsedReport.ReportedCase.CountMalesBelowFive}, " +
                                    $"males at least five: {parsedReport.ReportedCase.CountMalesAtLeastFive}, " +
                                    $"females below five: {parsedReport.ReportedCase.CountFemalesBelowFive}, " +
                                    $"females at least five: {parsedReport.ReportedCase.CountFemalesAtLeastFive}).");

                isValid = false;
            }

            if (projectHealthRisk != null)
            {
                switch (parsedReport.ReportType)
                {
                    case ReportType.Single when projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Human:
                        _loggerAdapter.Warn($"A report of type '{ReportType.Single}' has to be related to '{HealthRiskType.Human}' health risk only.");
                        isValid = false;
                        break;
                    case ReportType.Aggregate when projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Human:
                        _loggerAdapter.Warn($"A report of type '{ReportType.Aggregate}' has to be related to '{HealthRiskType.Human}' health risk only.");
                        isValid = false;
                        break;
                    case ReportType.NonHuman when projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.NonHuman && projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.UnusualEvent:
                        _loggerAdapter.Warn($"A report of type '{ReportType.NonHuman}' has to be related to '{HealthRiskType.NonHuman}' or '{HealthRiskType.UnusualEvent}' event only.");
                        isValid = false;
                        break;
                    case ReportType.Activity when projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity:
                        _loggerAdapter.Warn($"A report of type '{ReportType.Activity}' has to be related to '{HealthRiskType.Activity}' event only.");
                        isValid = false;
                        break;
                    case ReportType.DataCollectionPoint:
                        //ToDo: implement DataCollectionPoint logic
                        break;
                    case ReportType.Unknown:
                        isValid = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return (isValid, projectHealthRisk);
        }

        private DateTime? ParseTimestamp(string timestamp, string timeZoneName)
        {
            DateTime utcDateTime;

            try
            {
                var formatProvider = CultureInfo.InvariantCulture;
                const string timestampFormat = "yyyyMMddHHmmss";

                var timeZone = _dateTimeProvider.GetTimeZoneInfo(timeZoneName);

                var parsedSuccessfully = DateTime.TryParseExact(timestamp, timestampFormat, formatProvider, DateTimeStyles.None, out var dateTime);

                if (!parsedSuccessfully)
                {
                    return null;
                }

                utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone);
            }
            catch (Exception e)
            {
                _loggerAdapter.Warn($"Cannot parse timestamp '{timestamp}' for time zone '{timeZoneName}'. Exception: {e.Message} Stack trace: {e.StackTrace}");
                return null;
            }
            
            return utcDateTime;
        }
    }
}
