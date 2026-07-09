using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Alerts.Queries;

public class GetListQuery : IRequest<Result<PaginatedList<AlertListItemResponseDto>>>
{
    public GetListQuery(int projectId, int pageNumber, AlertListFilterRequestDto filterRequestDto)
    {
        // Guard against null filter DTO
        ProjectId = projectId;
        PageNumber = pageNumber;
        FilterRequestDto = filterRequestDto ?? throw new ArgumentNullException(nameof(filterRequestDto));
    }

    private int ProjectId { get; }
    private int PageNumber { get; }
    private AlertListFilterRequestDto FilterRequestDto { get; }

    public class Handler : IRequestHandler<GetListQuery, Result<PaginatedList<AlertListItemResponseDto>>>
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly INyssWebConfig _config;

        public Handler(
            INyssContext nyssContext,
            INyssWebConfig config,
            IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext ?? throw new ArgumentNullException(nameof(nyssContext));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<Result<PaginatedList<AlertListItemResponseDto>>> Handle(GetListQuery request, CancellationToken cancellationToken)
        {
            var projectId = request.ProjectId;
            var filterRequestDto = request.FilterRequestDto;
            var pageNumber = request.PageNumber;

            var currentUserName = _authorizationService.GetCurrentUserName();

            var nationalSocietyId = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => p.NationalSocietyId)
                .SingleAsync(cancellationToken);

            var currentUserInfo = await _nyssContext.Users
                .FilterAvailable()
                .Where(u => u.EmailAddress == currentUserName)
                .Select(u => new
                {
                    u.Id,
                    u.Role,
                    OrganizationId = u.UserNationalSocieties != null ? u.UserNationalSocieties
                        .Where(uns => uns.NationalSocietyId == nationalSocietyId)
                        .Select(uns => (int?)uns.OrganizationId)
                        .FirstOrDefault()
                        : null
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (currentUserInfo == null)
            {
                return Error<PaginatedList<AlertListItemResponseDto>>(ResultKey.User.Common.UserNotFound);
            }

            var currentUserId = currentUserInfo.Id;
            var currentRole = currentUserInfo.Role;
            var currentUserOrganizationId = currentUserInfo.OrganizationId;

            HashSet<int>? orgUserIds = null;
            if (currentRole != Role.Administrator && currentUserOrganizationId.HasValue)
            {
                orgUserIds = (await _nyssContext.UserNationalSocieties
                        .Where(nsu => nsu.NationalSocietyId == nationalSocietyId
                            && nsu.OrganizationId == currentUserOrganizationId.Value)
                        .Select(nsu => nsu.UserId)
                        .ToListAsync(cancellationToken))
                    .ToHashSet();
            }

            var alertsQuery = _nyssContext.Alerts
                .FilterByProject(projectId)
                .FilterByHealthRisk(filterRequestDto.HealthRiskId)
                .FilterByArea(filterRequestDto.Locations)
                .FilterByStatus(filterRequestDto.Status)
                .FilterByDate(filterRequestDto.StartDate, filterRequestDto.EndDate)
                .Sort(filterRequestDto.OrderBy, filterRequestDto.SortAscending);

            var rowsPerPage = _config.PaginationRowsPerPage;

            var totalCount = await alertsQuery.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return Success(Enumerable.Empty<AlertListItemResponseDto>()
                    .AsPaginatedList(pageNumber, 0, rowsPerPage));
            }

            var alerts = await alertsQuery
                .Select(a => new
                {
                    a.Id,
                    CreatedAt = a.CreatedAt.AddHours(filterRequestDto.UtcOffset),
                    a.Status,
                    a.EscalatedOutcome,
                    a.Comments,
                    ReportCount = a.AlertReports.Count,

                    LastReport = a.AlertReports
                        .OrderByDescending(ar => ar.Report.Id)
                        .Select(ar => new
                        {
                            ZoneName = ar.Report.RawReport.Zone != null
                                ? ar.Report.RawReport.Zone.Name
                                : "",
                            VillageName = ar.Report.RawReport.Village != null
                                ? ar.Report.RawReport.Village.Name
                                : "",
                            DistrictName = ar.Report.RawReport.Village != null &&
                                           ar.Report.RawReport.Village.District != null
                                ? ar.Report.RawReport.Village.District.Name
                                : "",
                            RegionName = ar.Report.RawReport.Village != null &&
                                         ar.Report.RawReport.Village.District != null &&
                                         ar.Report.RawReport.Village.District.Region != null
                                ? ar.Report.RawReport.Village.District.Region.Name
                                : "",
                            DataCollectorExists = ar.Report.RawReport.DataCollector != null,
                            SupervisorId = ar.Report.RawReport.DataCollector != null && ar.Report.RawReport.DataCollector.Supervisor != null
                                ? (int?)ar.Report.RawReport.DataCollector.Supervisor.Id
                                : null,
                            HeadSupervisorId = ar.Report.RawReport.DataCollector != null && ar.Report.RawReport.DataCollector.HeadSupervisor != null
                                ? (int?)ar.Report.RawReport.DataCollector.HeadSupervisor.Id
                                : null
                        })
                        .FirstOrDefault(),

                    HealthRisk = a.ProjectHealthRisk == null ||
                            a.ProjectHealthRisk.HealthRisk == null ||
                            a.ProjectHealthRisk.Project == null ||
                            a.ProjectHealthRisk.Project.NationalSociety == null ||
                            a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage == null
                            ? ""
                            : a.ProjectHealthRisk.HealthRisk.LanguageContents
                                .Where(lc => lc.ContentLanguage != null &&
                                            lc.ContentLanguage.Id == a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                                .Select(lc => lc.Name)
                                .FirstOrDefault() ?? ""
                })
                                        .Page(pageNumber, rowsPerPage)
                                        .AsNoTracking()
                                        .AsSplitQuery()
                                        .ToListAsync(cancellationToken);

            var dtos = alerts
                .Select(a =>
                {
                    var isAnonymized = currentRole != Role.Administrator
                        && (a.LastReport == null
                            || !a.LastReport.DataCollectorExists
                            || orgUserIds == null
                            || !((a.LastReport.SupervisorId.HasValue && orgUserIds.Contains(a.LastReport.SupervisorId.Value))
                                || (a.LastReport.HeadSupervisorId.HasValue && orgUserIds.Contains(a.LastReport.HeadSupervisorId.Value))));

                    return new AlertListItemResponseDto
                    {
                        Id = a.Id,
                        CreatedAt = a.CreatedAt,
                        Status = a.Status.ToString(),
                        EscalatedOutcome = a.EscalatedOutcome,
                        Comments = a.Comments ?? "",
                        ReportCount = a.ReportCount,

                        LastReportZone = a.LastReport?.ZoneName ?? "",
                        LastReportVillage = !isAnonymized
                            ? a.LastReport?.VillageName ?? ""
                            : "",
                        LastReportDistrict = a.LastReport?.DistrictName ?? "",
                        LastReportRegion = a.LastReport?.RegionName ?? "",
                        HealthRisk = a.HealthRisk
                    };
                })
                .AsPaginatedList(pageNumber, totalCount, rowsPerPage);

            return Success(dtos);
        }
    }
}