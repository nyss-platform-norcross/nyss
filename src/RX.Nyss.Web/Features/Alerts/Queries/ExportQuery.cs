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
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Alerts.Commands;

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

            var currentRole = (await _authorizationService.GetCurrentUser()).Role;
            var currentUserName = _authorizationService.GetCurrentUserName();
            var currentUserId = await _nyssContext.Users.FilterAvailable()
                .Where(u => u.EmailAddress == currentUserName)
                .Select(u => u.Id)
                .SingleAsync();
            var currentUserOrganizationId = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.NationalSociety.NationalSocietyUsers)
                .Where(uns => uns.User.Id == currentUserId)
                .Select(uns => uns.OrganizationId)
                .SingleOrDefaultAsync();

            var strings = await _stringsService.GetForCurrentUser();

            var alertsQuery = _nyssContext.Alerts
                .FilterByProject(projectId)
                .FilterByHealthRisk(filterRequestDto.HealthRiskId)
                .FilterByArea(filterRequestDto.Locations)
                .FilterByStatus(filterRequestDto.Status)
                .FilterByDate(filterRequestDto.StartDate, filterRequestDto.EndDate)
                .Sort(filterRequestDto.OrderBy, filterRequestDto.SortAscending);

            var alerts = await alertsQuery
                .Select(a => new
                {
                    a.Id,
                    a.CreatedAt,
                    a.EscalatedAt,
                    a.DismissedAt,
                    a.ClosedAt,
                    a.Status,
                    a.EscalatedOutcome,
                    a.Comments,
                    ReportCount = a.AlertReports.Count,
                    LastReport = a.AlertReports.OrderByDescending(ar => ar.Report.Id)
                        .Select(ar => new
                        {
                            ZoneName = ar.Report.RawReport.Zone.Name,
                            VillageName = ar.Report.RawReport.Village.Name,
                            DistrictName = ar.Report.RawReport.Village.District.Name,
                            RegionName = ar.Report.RawReport.Village.District.Region.Name,
                            Timestamp = ar.Report.ReceivedAt,
                            IsAnonymized = currentRole != Role.Administrator && !ar.Report.RawReport.NationalSociety.NationalSocietyUsers.Any(
                                nsu => nsu.UserId == ar.Report.RawReport.DataCollector.Supervisor.Id && nsu.OrganizationId == currentUserOrganizationId)
                        }).First(),
                    HealthRisk = a.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .Single(),
                    InvestigationEventSubtype = _nyssContext.AlertEventLogs
                        .Where(log => log.AlertId == a.Id && log.AlertEventType.Id == 1)
                        .Select(log => log.AlertEventSubtype)
                        .First(),
                    OutcomeEventSubtype = _nyssContext.AlertEventLogs
                        .Where(log => log.AlertId == a.Id && log.AlertEventType.Id == 4)
                        .Select(log => log.AlertEventSubtype)
                        .First()
                })
                .ToListAsync();

            var dtos = alerts
                .Select(a => new AlertListExportResponseDto
                {
                    Id = a.Id,
                    LastReportTimestamp = a.LastReport.Timestamp.AddHours(filterRequestDto.UtcOffset),
                    TriggeredAt = a.CreatedAt.AddHours(filterRequestDto.UtcOffset),
                    EscalatedAt = a.EscalatedAt?.AddHours(filterRequestDto.UtcOffset),
                    DismissedAt = a.DismissedAt?.AddHours(filterRequestDto.UtcOffset),
                    ClosedAt = a.ClosedAt?.AddHours(filterRequestDto.UtcOffset),
                    Status = strings[$"alerts.alertStatus.{a.Status.ToString().ToLower()}"],
                    EscalatedOutcome = a.EscalatedOutcome,
                    Comments = a.Comments,
                    ReportCount = a.ReportCount,
                    LastReportVillage = a.LastReport.IsAnonymized
                        ? ""
                        : a.LastReport.VillageName,
                    LastReportDistrict = a.LastReport.DistrictName,
                    LastReportRegion = a.LastReport.RegionName,
                    HealthRisk = a.HealthRisk,
                    Investigation = a.InvestigationEventSubtype != null
                        ? strings[$"alerts.eventTypes.subtypes.{a.InvestigationEventSubtype.Name.ToString().ToCamelCase()}"]
                        : null,
                    Outcome = a.OutcomeEventSubtype != null
                        ? strings[$"alerts.eventTypes.subtypes.{a.OutcomeEventSubtype.Name.ToString().ToCamelCase()}"]
                        : null
                }).ToList();

            var documentTitle = strings["alerts.export.title"];
            var columnLabels = GetColumnLabels(strings);
            var excelDoc = _excelExportService.ToExcel(dtos, columnLabels, documentTitle);

            return excelDoc.GetAsByteArray();
        }

        private static IReadOnlyList<string> GetColumnLabels(StringsResourcesVault strings) =>
            new []
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
                strings["alerts.export.escalatedOutcome"],
                strings["alerts.export.closedComments"],
            };
    }
}
