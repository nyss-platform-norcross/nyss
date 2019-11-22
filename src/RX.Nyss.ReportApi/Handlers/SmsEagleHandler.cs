using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
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

        private static readonly string SmsPattern = @$"{SenderParameterName}";
        private static readonly Regex CompiledSmsRegex = new Regex(SmsPattern, RegexOptions.Compiled);

        private readonly INyssReportContext _nyssReportContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IDateTimeProvider _dateTimeProvider;

        public SmsEagleHandler(INyssReportContext nyssReportContext, ILoggerAdapter loggerAdapter, IDateTimeProvider dateTimeProvider)
        {
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
            var messageId = parsedQueryString[MessageIdParameterName].ParseToNullableInt();
            var outgoingMessageId = parsedQueryString[OutgoingMessageIdParameterName];
            var modemNumber = parsedQueryString[ModemNumberParameterName].ParseToNullableInt();
            var apiKey = parsedQueryString[ApiKeyParameterName];

            var gatewaySettings = await _nyssReportContext.GatewaySettings
                .SingleOrDefaultAsync(gs => gs.ApiKey == apiKey);

            if (gatewaySettings.GatewayType != GatewayType.SmsEagle)
            {
                _loggerAdapter.Error("");
            }

            var dataCollector = await _nyssReportContext.DataCollectors
                .Include(dc => dc.Project)
                .SingleOrDefaultAsync(dc => dc.PhoneNumber == sender || dc.AdditionalPhoneNumber == sender);

            if (dataCollector.Project.NationalSocietyId != gatewaySettings.NationalSocietyId)
            {
                _loggerAdapter.Error("");
            }

            var receivedAt = ParseTimestamp(timestamp, dataCollector.Project.TimeZone);

            if (receivedAt > _dateTimeProvider.UtcNow)
            {
                _loggerAdapter.Error("");
            }

            (ReportType reportType, bool isValid, ReportCase reportedCase, DataCollectionPointCase dataCollectionPointCase) = ParseTextMessage(text);
            
            var report = new Report
            {
                RawContent = text,
                IsValid = isValid,
                IsTraining = isTraining,
                ReportType = reportType,
                Status = ReportStatus.Pending,
                ReceivedAt = receivedAt,
                CreatedAt = _dateTimeProvider.UtcNow,
                DataCollector = dataCollector,
                Location = dataCollector.Location,
                ReportedCase = reportedCase,
                KeptCase = new ReportCase
                {
                    CountMalesBelowFive = null,
                    CountMalesAtLeastFive = null,
                    CountFemalesBelowFive = null,
                    CountFemalesAtLeastFive = null
                },
                DataCollectionPointCase = dataCollectionPointCase,
                ProjectHealthRisk = new ProjectHealthRisk
                {
                    
                }
            };

            _nyssReportContext.Reports.Add(report);

            await _nyssReportContext.SaveChangesAsync();
        }

        private DateTime ParseTimestamp(string timestamp, string timeZone)
        {
            return new DateTime();
        }
    }
}
