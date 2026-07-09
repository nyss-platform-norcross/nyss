using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Features.Reports.Exceptions;
using RX.Nyss.ReportApi.Features.Reports.Models;

namespace RX.Nyss.ReportApi.Features.Reports;

public interface IReportValidationService
{
    Task<ProjectHealthRisk> ValidateReport(ParsedReport parsedReport, DataCollector dataCollector, int nationalSocietyId);
    DateTime ParseTimestamp(string timestamp);
    DateTime ParseTelerivetTimestamp(int unixTimeStamp);
    void ValidateReceivalTime(DateTime receivedAt);
    Task<GatewaySetting> ValidateGatewaySetting(string apiKey);

    Task<GatewaySetting> ValidateMTNGatewaySetting(string senderId);
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

    public async Task<ProjectHealthRisk> ValidateReport(ParsedReport parsedReport, DataCollector dataCollector, int nationalSocietyId)
    {
        var projectHealthRiskData = await _nyssContext.ProjectHealthRisks
            .Include(phr => phr.HealthRisk)
            .Include(phr => phr.Project)
            .FirstOrDefaultAsync(phr =>
                phr.HealthRisk.HealthRiskCode == parsedReport.HealthRiskCode &&
                (
                    (dataCollector != null && phr.Project.Id == dataCollector.Project.Id) ||
                    (dataCollector == null && phr.Project.NationalSocietyId == nationalSocietyId)
                ));

        var healthRiskType = projectHealthRiskData?.HealthRisk?.HealthRiskType;

        if (projectHealthRiskData == null || !healthRiskType.HasValue)
        {
            if (dataCollector != null)
            {
                throw new ReportValidationException(
                    $"A health risk with code '{parsedReport.HealthRiskCode}' is not listed in project with id '{dataCollector.Project.Id}'.",
                    ReportErrorType.HealthRiskNotFound);
            }

            throw new ReportValidationException(
                $"A health risk with code '{parsedReport.HealthRiskCode}' is not listed in national society with id '{nationalSocietyId}'.",
                ReportErrorType.HealthRiskNotFound);
        }

        if (dataCollector != null)
        {
            switch (dataCollector.DataCollectorType)
            {
                case DataCollectorType.Human:
                    if (parsedReport.ReportType != ReportType.Single &&
                        parsedReport.ReportType != ReportType.Aggregate &&
                        parsedReport.ReportType != ReportType.Event)
                    {
                        throw new ReportValidationException($"A data collector of type '{DataCollectorType.Human}' can only send a report of type " +
                            $"'{ReportType.Single}', '{ReportType.Aggregate}', '{ReportType.Event}'.", ReportErrorType.DataCollectorUsedCollectionPointFormat);
                    }

                    break;
                case DataCollectorType.CollectionPoint:
                    if (parsedReport.ReportType != ReportType.DataCollectionPoint &&
                        parsedReport.ReportType != ReportType.Event)
                    {
                        throw new ReportValidationException($"A data collector of type '{DataCollectorType.CollectionPoint}' can only send a report of type " +
                            $"'{ReportType.DataCollectionPoint}', '{ReportType.Event}'.", ReportErrorType.CollectionPointUsedDataCollectorFormat);
                    }

                    break;
                default:
                    throw new ReportValidationException($"A data collector of type '{dataCollector.DataCollectorType}' is not supported.");
            }
        }

        switch (parsedReport.ReportType)
        {
            case ReportType.Single:
                if (healthRiskType.Value != HealthRiskType.Human)
                {
                    throw new ReportValidationException($"A report of type '{ReportType.Single}' has to be related to '{HealthRiskType.Human}' health risk only.", ReportErrorType.SingleReportNonHumanHealthRisk);
                }

                break;
            case ReportType.Aggregate:
                if (healthRiskType.Value != HealthRiskType.Human)
                {
                    throw new ReportValidationException($"A report of type '{ReportType.Aggregate}' has to be related to '{HealthRiskType.Human}' health risk only.", ReportErrorType.AggregateReportNonHumanHealthRisk);
                }

                break;
            case ReportType.Event:
                if (healthRiskType.Value != HealthRiskType.NonHuman &&
                    healthRiskType.Value != HealthRiskType.UnusualEvent &&
                    healthRiskType.Value != HealthRiskType.Activity)
                {
                    throw new ReportValidationException(
                        $"A report of type '{ReportType.Event}' has to be related to '{HealthRiskType.NonHuman}' or '{HealthRiskType.UnusualEvent}' or '{HealthRiskType.Activity}' event only.", ReportErrorType.EventReportHumanHealthRisk);
                }

                break;
            case ReportType.DataCollectionPoint:
                if (healthRiskType.Value != HealthRiskType.Human)
                {
                    throw new ReportValidationException(
                        $"A report of type '{ReportType.DataCollectionPoint}' has to be related to '{HealthRiskType.Human}' health risk only.", ReportErrorType.CollectionPointNonHumanHealthRisk);
                }

                break;
            default:
                throw new ReportValidationException($"A report of type '{parsedReport.ReportType}' is not supported.");
        }

        if (projectHealthRiskData.HealthRisk == null)
        {
            throw new Exception(
                $"HealthRisk navigation property was not loaded for ProjectHealthRisk Id={projectHealthRiskData.Id}");
        }

        return projectHealthRiskData;
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
            throw new ReportValidationException($"Cannot parse timestamp '{timestamp}'. Exception: {e.Message} Stack trace: {e.StackTrace}", ReportErrorType.Gateway);
        }
    }

    public DateTime ParseTelerivetTimestamp(int unixTimeStamp)
    {
        try
        {
            // Unix timestamp is seconds past epoch
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();

            var parsedTimestampInUtc = dateTime.ToUniversalTime();
            return parsedTimestampInUtc;
        }
        catch (Exception e)
        {
            throw new ReportValidationException($"Cannot parse timestamp '{unixTimeStamp}'. Exception: {e.Message} Stack trace: {e.StackTrace}", ReportErrorType.Timestamp);
        }
    }

    public void ValidateReceivalTime(DateTime receivedAt)
    {
        const int maxAllowedPrecedenceInMinutes = 3;

        if (receivedAt > _dateTimeProvider.UtcNow.AddMinutes(maxAllowedPrecedenceInMinutes))
        {
            throw new ReportValidationException("The receival time cannot be in the future.", ReportErrorType.Timestamp);
        }
    }

    public async Task<GatewaySetting> ValidateGatewaySetting(string apiKey)
    {
        var gatewaySetting = await _nyssContext.GatewaySettings
            .Include(gs => gs.NationalSociety)
            .SingleOrDefaultAsync(gs => gs.ApiKey == apiKey);

        if (gatewaySetting == null)
        {
            throw new ReportValidationException($"A gateway setting with API key '{apiKey}' does not exist.", ReportErrorType.Gateway);
        }

        if (gatewaySetting.GatewayType != GatewayType.SmsEagle && gatewaySetting.GatewayType != GatewayType.SmsGateway && gatewaySetting.GatewayType != GatewayType.Telerivet && gatewaySetting.GatewayType != GatewayType.MTNSmsGateway)
        {
            throw new ReportValidationException($"A gateway type ('{gatewaySetting.GatewayType}') is different than '{GatewayType.SmsEagle}' or '{GatewayType.SmsGateway}' or '{GatewayType.Telerivet}' or '{GatewayType.MTNSmsGateway}'.", ReportErrorType.Gateway);
        }

        return gatewaySetting;
    }

    public async Task<GatewaySetting> ValidateMTNGatewaySetting(string senderId)
    {
        var gatewaySetting = await _nyssContext.GatewaySettings
            .Include(gs => gs.NationalSociety)
            .SingleOrDefaultAsync(gs => gs.GatewaySenderId == senderId);

        if (gatewaySetting == null)
        {
            throw new ReportValidationException($"A gateway setting with senderID '{senderId}' does not exist.", ReportErrorType.Gateway);
        }

        if (gatewaySetting.GatewayType != GatewayType.MTNSmsGateway)
        {
            throw new ReportValidationException($"A gateway type ('{gatewaySetting.GatewayType}') is different than '{GatewayType.MTNSmsGateway}'.", ReportErrorType.Gateway);
        }

        return gatewaySetting;
    }
}