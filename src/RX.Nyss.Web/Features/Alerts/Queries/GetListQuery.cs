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
        ProjectId = projectId;
        PageNumber = pageNumber;
        FilterRequestDto = filterRequestDto;
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
            IAuthorizationService authorizationService
        )
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
            _config = config;
        }

        public async Task<Result<PaginatedList<AlertListItemResponseDto>>> Handle(GetListQuery request, CancellationToken cancellationToken)
        {
            var projectId = request.ProjectId;
            var filterRequestDto = request.FilterRequestDto;
            var pageNumber = request.PageNumber;

            var alertsQuery = _nyssContext.Alerts
                .FilterByProject(projectId)
                .FilterByHealthRisk(filterRequestDto.HealthRiskId)
                .FilterByArea(filterRequestDto.Locations)
                .FilterByStatus(filterRequestDto.Status)
                .FilterByDate(filterRequestDto.StartDate, filterRequestDto.EndDate)
                .Sort(filterRequestDto.OrderBy, filterRequestDto.SortAscending);

            var rowsPerPage = _config.PaginationRowsPerPage;
            var totalCount = await alertsQuery.CountAsync();
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

            var alerts = await alertsQuery
                .Select(a => new
                {
                    a.Id,
                    CreatedAt = a.CreatedAt.AddHours(filterRequestDto.UtcOffset),
                    a.Status,
                    a.EscalatedOutcome,
                    a.Comments,
                    ReportCount = a.AlertReports.Count,
                    LastReport = a.AlertReports.OrderByDescending(ar => ar.Report.Id)
                        .Select(ar => new
                        {
                            VillageName = ar.Report.RawReport.Village.Name,
                            DistrictName = ar.Report.RawReport.Village.District.Name,
                            RegionName = ar.Report.RawReport.Village.District.Region.Name,
                            IsAnonymized = currentRole != Role.Administrator && !ar.Report.RawReport.NationalSociety.NationalSocietyUsers.Any(
                                nsu => nsu.UserId == ar.Report.RawReport.DataCollector.Supervisor.Id && nsu.OrganizationId == currentUserOrganizationId)
                        }).First(),
                    HealthRisk = a.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .Single()
                })
                .Page(pageNumber, rowsPerPage)
                .AsNoTracking()
                .ToListAsync();

            var dtos = alerts
                .Select(a => new AlertListItemResponseDto
                {
                    Id = a.Id,
                    CreatedAt = a.CreatedAt,
                    Status = a.Status.ToString(),
                    EscalatedOutcome = a.EscalatedOutcome,
                    Comments = a.Comments,
                    ReportCount = a.ReportCount,
                    LastReportVillage = a.LastReport.IsAnonymized
                        ? ""
                        : a.LastReport.VillageName,
                    LastReportDistrict = a.LastReport.DistrictName,
                    LastReportRegion = a.LastReport.RegionName,
                    HealthRisk = a.HealthRisk
                })
                .AsPaginatedList(pageNumber, totalCount, rowsPerPage);

            return Success(dtos);
        }
    }
}
