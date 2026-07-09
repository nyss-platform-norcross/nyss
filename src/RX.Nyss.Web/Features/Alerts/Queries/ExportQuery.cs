using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Alerts.Queries;

public class ExportQuery : IRequest<byte[]>
{
    public ExportQuery(int projectId, AlertListFilterRequestDto filterRequestDto)
    {
        ProjectId = projectId;
        FilterRequestDto = filterRequestDto;
    }

    private int ProjectId { get; }
    private AlertListFilterRequestDto FilterRequestDto { get; }

    public class Handler : IRequestHandler<ExportQuery, byte[]>
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IExcelExportService _excelExportService;
        private readonly IStringsService _stringsService;

        private const int ExportIdBatchSize = 500;

        private const int InvestigationEventTypeId = 1;
        private const int OutcomeEventTypeId = 4;
        private const int SummaryEventTypeId = 5;
        private const int PublicHealthActionEventTypeId = 6;

        private sealed record EventLogEntry(int AlertId, int EventTypeId, string? Text, string? SubtypeName);

        public Handler(
            INyssContext nyssContext,
            IAuthorizationService authorizationService,
            IExcelExportService excelExportService,
            IStringsService stringsService
        )
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
            _excelExportService = excelExportService;
            _stringsService = stringsService;
        }

        public async Task<byte[]> Handle(ExportQuery request, CancellationToken cancellationToken)
        {
            var projectId = request.ProjectId;
            var filterRequestDto = request.FilterRequestDto;

            var currentUser = await _authorizationService.GetCurrentUser();
            var currentRole = currentUser?.Role ?? Role.DataConsumer;
            var currentUserName = _authorizationService.GetCurrentUserName();

            var currentUserId = await _nyssContext.Users
                .FilterAvailable()
                .Where(u => u.EmailAddress == currentUserName)
                .Select(u => u.Id)
                .SingleAsync(cancellationToken);

            var projectInfo = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => new
                {
                    p.NationalSocietyId,
                    ContentLanguageId = p.NationalSociety!.ContentLanguage!.Id,
                    CurrentUserOrganizationId = p.NationalSociety.NationalSocietyUsers!
                        .Where(uns => uns.User != null && uns.User.Id == currentUserId)
                        .Select(uns => (int?)uns.OrganizationId)
                        .FirstOrDefault()
                })
                .SingleAsync(cancellationToken);

            var strings = await _stringsService.GetForCurrentUser();

            var baseQuery = _nyssContext.Alerts
                .FilterByProject(projectId)
                .FilterByHealthRisk(filterRequestDto.HealthRiskId)
                .FilterByArea(filterRequestDto.Locations)
                .FilterByStatus(filterRequestDto.Status)
                .FilterByDate(filterRequestDto.StartDate, filterRequestDto.EndDate)
                .Sort(filterRequestDto.OrderBy, filterRequestDto.SortAscending);

            var alertIds = await baseQuery
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);

            if (alertIds.Count == 0)
                return _excelExportService
                    .ToExcel(new List<AlertListExportResponseDto>(), GetColumnLabels(strings), strings["alerts.export.title"])
                    .GetAsByteArray();

            HashSet<int>? orgUserIds = null;
            if (currentRole != Role.Administrator && projectInfo.CurrentUserOrganizationId.HasValue)
            {
                orgUserIds = (await _nyssContext.UserNationalSocieties
                        .Where(nsu => nsu.NationalSocietyId == projectInfo.NationalSocietyId
                            && nsu.OrganizationId == projectInfo.CurrentUserOrganizationId.Value)
                        .Select(nsu => nsu.UserId)
                        .ToListAsync(cancellationToken))
                    .ToHashSet();
            }

            var relevantTypeIds = new[]
            {
                InvestigationEventTypeId,
                OutcomeEventTypeId,
                SummaryEventTypeId,
                PublicHealthActionEventTypeId
            };

            var idBatches = alertIds.Chunk(ExportIdBatchSize).Select(b => b.ToList()).ToList();

            var eventLogsByAlert = new Dictionary<int, Dictionary<int, EventLogEntry>>();
            foreach (var batchIds in idBatches)
            {
                var batchLogs = await _nyssContext.AlertEventLogs
                    .Where(log => batchIds.Contains(log.AlertId) && relevantTypeIds.Contains(log.AlertEventType.Id))
                    .Select(log => new EventLogEntry(
                        log.AlertId,
                        log.AlertEventType.Id,
                        log.Text,
                        log.AlertEventSubtype != null ? log.AlertEventSubtype.Name : null
                    ))
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                foreach (var alertGroup in batchLogs.GroupBy(l => l.AlertId))
                {
                    eventLogsByAlert[alertGroup.Key] = alertGroup
                        .GroupBy(l => l.EventTypeId)
                        .ToDictionary(typeGroup => typeGroup.Key, typeGroup => typeGroup.First());
                }
            }

            var alertsById = new Dictionary<int, AlertExportRow>(alertIds.Count);
            foreach (var batchIds in idBatches)
            {
                var batchRows = await SelectExportProjection(
                        _nyssContext.Alerts.AsNoTracking().Where(a => batchIds.Contains(a.Id)),
                        projectInfo.ContentLanguageId)
                    .ToListAsync(cancellationToken);

                foreach (var row in batchRows)
                    alertsById[row.Id] = row;
            }

            var utcOffset = filterRequestDto.UtcOffset;

            var dtos = alertIds
                .Where(id => alertsById.ContainsKey(id))
                .Select(id => alertsById[id])
                .Select(a =>
                {
                    eventLogsByAlert.TryGetValue(a.Id, out var logsByType);

                    EventLogEntry? investigationLog = null;
                    EventLogEntry? outcomeLog = null;
                    EventLogEntry? summaryLog = null;
                    EventLogEntry? publicHealthLog = null;

                    if (logsByType != null)
                    {
                        logsByType.TryGetValue(InvestigationEventTypeId, out investigationLog);
                        logsByType.TryGetValue(OutcomeEventTypeId, out outcomeLog);
                        logsByType.TryGetValue(SummaryEventTypeId, out summaryLog);
                        logsByType.TryGetValue(PublicHealthActionEventTypeId, out publicHealthLog);
                    }

                    var isAnonymized = IsLastReportAnonymized(
                        currentRole,
                        a.LastReportDataCollectorExists,
                        a.LastReportSupervisorId,
                        a.LastReportHeadSupervisorId,
                        orgUserIds);

                    return new AlertListExportResponseDto
                    {
                        Id = a.Id,
                        TriggeredAt = a.CreatedAt.AddHours(utcOffset),
                        EscalatedAt = a.EscalatedAt?.AddHours(utcOffset),
                        DismissedAt = a.DismissedAt?.AddHours(utcOffset),
                        ClosedAt = a.ClosedAt?.AddHours(utcOffset),
                        Status = strings[$"alerts.alertStatus.{a.Status.ToString().ToLower()}"],
                        EscalatedOutcome = a.EscalatedOutcome,
                        ReportCount = a.ReportCount,
                        HealthRisk = a.HealthRisk ?? string.Empty,

                        LastReportTimestamp = a.LastReportTimestamp?.AddHours(utcOffset) ?? default,
                        LastReportZone = a.LastReportZoneName,
                        LastReportVillage = isAnonymized ? string.Empty : a.LastReportVillageName,
                        LastReportDistrict = a.LastReportDistrictName,
                        LastReportRegion = a.LastReportRegionName,

                        InvestigationComment = investigationLog?.Text,
                        OutcomeComment = outcomeLog?.Text,
                        PublicHealthActionComment = publicHealthLog?.Text,
                        SummaryComment = summaryLog?.Text,

                        Investigation = investigationLog?.SubtypeName is { } invSub
                            ? strings[$"alerts.eventTypes.subtypes.{invSub.ToString().ToCamelCase()}"]
                            : null,
                        Outcome = outcomeLog?.SubtypeName is { } outSub
                            ? strings[$"alerts.eventTypes.subtypes.{outSub.ToString().ToCamelCase()}"]
                            : null,
                        PublicHealthActionTaken = publicHealthLog?.SubtypeName is { } phaSub
                            ? strings[$"alerts.eventTypes.subtypes.{phaSub.ToString().ToCamelCase()}"]
                            : null
                    };
                })
                .ToList();

            var documentTitle = strings["alerts.export.title"];
            var columnLabels = GetColumnLabels(strings);
            var excelDoc = _excelExportService.ToExcel(dtos, columnLabels, documentTitle);

            return excelDoc.GetAsByteArray();
        }

        private static bool IsLastReportAnonymized(
            Role currentRole,
            bool dataCollectorExists,
            int? supervisorId,
            int? headSupervisorId,
            HashSet<int>? orgUserIds) =>
            currentRole != Role.Administrator
            && (!dataCollectorExists
                || orgUserIds == null
                || !((supervisorId.HasValue && orgUserIds.Contains(supervisorId.Value))
                    || (headSupervisorId.HasValue && orgUserIds.Contains(headSupervisorId.Value))));

        private static IQueryable<AlertExportRow> SelectExportProjection(
            IQueryable<Alert> query,
            int contentLanguageId) =>
            query.Select(a => new AlertExportRow
            {
                Id = a.Id,
                CreatedAt = a.CreatedAt,
                EscalatedAt = a.EscalatedAt,
                DismissedAt = a.DismissedAt,
                ClosedAt = a.ClosedAt,
                Status = a.Status,
                EscalatedOutcome = a.EscalatedOutcome,
                ReportCount = a.AlertReports.Count,
                HealthRisk = a.ProjectHealthRisk!.HealthRisk!.LanguageContents
                    .Where(lc => lc.ContentLanguage.Id == contentLanguageId)
                    .Select(lc => lc.Name)
                    .FirstOrDefault(),
                LastReport = a.AlertReports
                    .OrderByDescending(ar => ar.Report.Id)
                    .Select(ar => new LastReportProjection
                    {
                        Timestamp = ar.Report.ReceivedAt,
                        ZoneName = ar.Report.RawReport.Zone != null ? ar.Report.RawReport.Zone.Name : null,
                        VillageName = ar.Report.RawReport.Village != null ? ar.Report.RawReport.Village.Name : null,
                        DistrictName = ar.Report.RawReport.Village != null && ar.Report.RawReport.Village.District != null
                            ? ar.Report.RawReport.Village.District.Name
                            : null,
                        RegionName = ar.Report.RawReport.Village != null
                            && ar.Report.RawReport.Village.District != null
                            && ar.Report.RawReport.Village.District.Region != null
                                ? ar.Report.RawReport.Village.District.Region.Name
                                : null,
                        DataCollectorExists = ar.Report.RawReport.DataCollector != null,
                        SupervisorId = ar.Report.RawReport.DataCollector != null && ar.Report.RawReport.DataCollector.Supervisor != null
                            ? (int?)ar.Report.RawReport.DataCollector.Supervisor.Id
                            : null,
                        HeadSupervisorId = ar.Report.RawReport.DataCollector != null && ar.Report.RawReport.DataCollector.HeadSupervisor != null
                            ? (int?)ar.Report.RawReport.DataCollector.HeadSupervisor.Id
                            : null
                    })
                    .FirstOrDefault()
            });

        private sealed class LastReportProjection
        {
            public DateTime Timestamp { get; init; }
            public string? ZoneName { get; init; }
            public string? VillageName { get; init; }
            public string? DistrictName { get; init; }
            public string? RegionName { get; init; }
            public bool DataCollectorExists { get; init; }
            public int? SupervisorId { get; init; }
            public int? HeadSupervisorId { get; init; }
        }

        private sealed class AlertExportRow
        {
            public int Id { get; init; }
            public DateTime CreatedAt { get; init; }
            public DateTime? EscalatedAt { get; init; }
            public DateTime? DismissedAt { get; init; }
            public DateTime? ClosedAt { get; init; }
            public AlertStatus Status { get; init; }
            public EscalatedAlertOutcomes? EscalatedOutcome { get; init; }
            public int ReportCount { get; init; }
            public string? HealthRisk { get; init; }
            public LastReportProjection? LastReport { get; init; }

            public DateTime? LastReportTimestamp => LastReport?.Timestamp;
            public string? LastReportZoneName => LastReport?.ZoneName;
            public string? LastReportVillageName => LastReport?.VillageName;
            public string? LastReportDistrictName => LastReport?.DistrictName;
            public string? LastReportRegionName => LastReport?.RegionName;
            public bool LastReportDataCollectorExists => LastReport?.DataCollectorExists ?? false;
            public int? LastReportSupervisorId => LastReport?.SupervisorId;
            public int? LastReportHeadSupervisorId => LastReport?.HeadSupervisorId;
        }

        private static IReadOnlyList<string> GetColumnLabels(StringsResourcesVault strings) =>
       new[]
       {
                strings["alerts.export.id"],
                strings["alerts.export.dateTriggered"],
                strings["alerts.export.timeTriggered"],
                strings["alerts.export.dateOfLastReport"],
                strings["alerts.export.timeOfLastReport"],
                strings["alerts.export.healthRisk"],
                strings["alerts.export.reports"],
                strings["alerts.export.status"],
                strings["alerts.export.lastReportRegion"],
                strings["alerts.export.lastReportDistrict"],
                strings["alerts.export.lastReportVillage"],
                strings["alerts.export.lastReportZone"],
                strings["alerts.export.dateEscalated"],
                strings["alerts.export.timeEscalated"],
                strings["alerts.export.dateClosed"],
                strings["alerts.export.timeClosed"],
                strings["alerts.export.dateDismissed"],
                strings["alerts.export.timeDismissed"],
                strings["alerts.export.investigation"],
                strings["alerts.export.outcome"],
                strings["alerts.export.publicHealthAction"],
                strings["alerts.export.investigationComment"],
                strings["alerts.export.outcomeComment"],
                strings["alerts.export.summaryComment"],
                strings["alerts.export.phatComment"]
       };
    }
}