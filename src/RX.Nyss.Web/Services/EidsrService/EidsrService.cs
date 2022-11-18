﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RX.Nyss.Common.Services.EidsrClient;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Services.EidsrClient.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Services.EidsrService;

public interface IEidsrService
{
    Task<Result<EidsrOrganisationUnitsResponse>> GetOrganizationUnits(EidsrApiProperties apiProperties, string programId);

    Task<Result<EidsrProgramResponse>> GetProgram(EidsrApiProperties apiProperties, string programId);

    Task SendReportsToEidsr(List<int> reportsId);
}

public class EidsrService : IEidsrService
{
    private readonly IInMemoryCache _inMemoryCache;
    private readonly IEidsrClient _eidsrClient;
    private readonly INyssContext _nyssContext;
    private readonly ILoggerAdapter _loggerAdapter;
    private readonly IQueueService _queueService;
    private readonly INyssWebConfig _config;

    public EidsrService(
        IInMemoryCache inMemoryCache,
        IEidsrClient eidsrClient,
        INyssContext nyssContext,
        IQueueService queueService,
        ILoggerAdapter loggerAdapter,
        INyssWebConfig config
        )
    {
        _inMemoryCache = inMemoryCache;
        _eidsrClient = eidsrClient;
        _nyssContext = nyssContext;
        _queueService = queueService;
        _loggerAdapter = loggerAdapter;
        _config = config;
    }

    public async Task<Result<EidsrOrganisationUnitsResponse>> GetOrganizationUnits(EidsrApiProperties apiProperties, string programId)
    {
        _inMemoryCache.Remove($"Eidsr_OrganizationUnits_{programId}");
        return await _inMemoryCache.GetCachedResult(
            key: $"Eidsr_OrganizationUnits_{programId}",
            validFor: TimeSpan.FromSeconds(2),
            value: () => _eidsrClient.GetOrganizationUnits(apiProperties, programId));
    }

    public Task<Result<EidsrProgramResponse>> GetProgram(EidsrApiProperties apiProperties, string programId) =>
        _eidsrClient.GetProgram(apiProperties, programId);

    public async Task SendReportsToEidsr(List<int> reportsId)
    {
        try
        {
            await _queueService.SendCollection(
                _config.ServiceBusQueues.EidsrReportQueue,
                reportsId.Select(x => new EidsrReport { ReportId = x }).ToList());
        }
        catch (Exception e)
        {
            _loggerAdapter.Error(e, $"Failed to SendReportsToEidsr");
            throw new ResultException(ResultKey.Alert.EscalateAlert.EmailNotificationFailed);
        }
    }

    public void ValidateEidsrReports(List<int> reportsId)
    {
        // TODO: Important! call it from the frontend within Escalate component

        // Get EidsrConfiguration

        // Ping Client

        // All districts have EidsUnit
    }
}