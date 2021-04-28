using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Features.Reports.Contracts;
using RX.Nyss.ReportApi.Features.Reports.Exceptions;
using RX.Nyss.ReportApi.Features.Reports.Models;

namespace RX.Nyss.ReportApi.Features.Reports
{
    public interface IReportValidationService
    {
        Task<ProjectHealthRisk> ValidateReport(ParsedReport parsedReport, DataCollector dataCollector);
        DateTime ParseTimestamp(string timestamp);
        void ValidateReceivalTime(DateTime receivedAt);
    }

    public class ReportValidationService : IReportValidationService
    {
        private readonly INyssContext _nyssContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILoggerAdapter _loggerAdapter;

        public ReportValidationService(INyssContext nyssContext, IDateTimeProvider dateTimeProvider, ILoggerAdapter loggerAdapter)
        {
            _nyssContext = nyssContext;
            _dateTimeProvider = dateTimeProvider;
            _loggerAdapter = loggerAdapter;
        }

        public async Task<ProjectHealthRisk> ValidateReport(ParsedReport parsedReport, DataCollector dataCollector)
        {
            var projectHealthRisk = await _nyssContext.ProjectHealthRisks
                .Include(phr => phr.HealthRisk)
                .SingleOrDefaultAsync(phr => phr.HealthRisk.HealthRiskCode == parsedReport.HealthRiskCode &&
                    phr.Project.Id == dataCollector.Project.Id);

            if (projectHealthRisk == null)
            {
                throw new ReportValidationException($"A health risk with code '{parsedReport.HealthRiskCode}' is not listed in project with id '{dataCollector.Project.Id}'.",
                    ReportErrorType.HealthRiskNotFound);
            }

            switch (dataCollector.DataCollectorType)
            {
                case DataCollectorType.Human:
                    if (parsedReport.ReportType != ReportType.Single &&
                        parsedReport.ReportType != ReportType.Aggregate &&
                        parsedReport.ReportType != ReportType.Event)
                    {
                        throw new ReportValidationException($"A data collector of type '{DataCollectorType.Human}' can only send a report of type " +
                            $"'{ReportType.Single}', '{ReportType.Aggregate}', '{ReportType.Event}'.");
                    }

                    break;
                case DataCollectorType.CollectionPoint:
                    if (parsedReport.ReportType != ReportType.DataCollectionPoint &&
                        parsedReport.ReportType != ReportType.Event)
                    {
                        throw new ReportValidationException($"A data collector of type '{DataCollectorType.CollectionPoint}' can only send a report of type " +
                            $"'{ReportType.DataCollectionPoint}', '{ReportType.Event}.");
                    }

                    break;
                default:
                    throw new ReportValidationException($"A data collector of type '{dataCollector.DataCollectorType}' is not supported.");
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
                case ReportType.Event:
                    if (projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.NonHuman &&
                        projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.UnusualEvent &&
                        projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity)
                    {
                        throw new ReportValidationException(
                            $"A report of type '{ReportType.Event}' has to be related to '{HealthRiskType.NonHuman}' or '{HealthRiskType.UnusualEvent}' or '{HealthRiskType.Activity}' event only.");
                    }

                    break;
                case ReportType.DataCollectionPoint:
                    if (projectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Human)
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

        public DateTime ParseTimestamp(string timestamp)
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

                var diffToNow = parsedTimestampInUtc - _dateTimeProvider.UtcNow;
                if (diffToNow > TimeSpan.FromMinutes(57) && diffToNow < TimeSpan.FromMinutes(63))
                {
                    _loggerAdapter.Warn($"Timestamp is {diffToNow.TotalHours:0.##} hour into the future, likely due to wrong timezone settings, please check eagle!");
                    parsedTimestampInUtc = parsedTimestampInUtc.AddHours(-1);
                }

                return parsedTimestampInUtc;
            }
            catch (Exception e)
            {
                throw new ReportValidationException($"Cannot parse timestamp '{timestamp}'. Exception: {e.Message} Stack trace: {e.StackTrace}");
            }
        }

        public void ValidateReceivalTime(DateTime receivedAt)
        {
            const int maxAllowedPrecedenceInMinutes = 3;

            if (receivedAt > _dateTimeProvider.UtcNow.AddMinutes(maxAllowedPrecedenceInMinutes))
            {
                throw new ReportValidationException("The receival time cannot be in the future.");
            }
        }
    }
}
