using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.NationalSocietyStructure;
using RX.Nyss.Web.Features.Projects;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Features.Users;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Services.EidsrService;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Reports;

public interface IReportService
{
    Task<Result<PaginatedList<ReportListResponseDto>>> List(int projectId, int pageNumber, ReportListFilterRequestDto filter);

    Task<Result<ReportListFilterResponseDto>> GetFilters(int nationalSocietyId);

    Task<Result<HumanHealthRiskResponseDto>> GetHumanHealthRisksForProject(int projectId);

    IQueryable<RawReport> GetRawReportsWithDataCollectorQuery(ReportsFilter filters);

    IQueryable<RawReport> GetRawReportsWithDataCollectorAndActivityReportsQuery(ReportsFilter filters);

    IQueryable<Report> GetDashboardHealthRiskEventReportsQuery(ReportsFilter filters);

    Task<Result> AcceptReport(int reportId);

    Task<Result> DismissReport(int reportId);
}

public class ReportService : IReportService
{
    private readonly INyssWebConfig _config;

    private readonly INyssContext _nyssContext;

    private readonly IUserService _userService;

    private readonly IProjectService _projectService;

    private readonly IAuthorizationService _authorizationService;

    private readonly IDateTimeProvider _dateTimeProvider;

    private readonly IStringsService _stringsService;

    private readonly INationalSocietyStructureService _nationalSocietyStructureService;

    private readonly IEidsrService _dhisService;

    private readonly ILoggerAdapter _loggerAdapter;

    private readonly IDashboardFilteredReportsScope _dashboardFilteredReportsScope;

    public ReportService(
        INyssContext nyssContext,
        IUserService userService,
        IProjectService projectService,
        INyssWebConfig config,
        IAuthorizationService authorizationService,
        IDateTimeProvider dateTimeProvider,
        IStringsService stringsService,
        IEidsrService dhisService,
        ILoggerAdapter loggerAdapter,
        INationalSocietyStructureService nationalSocietyStructureService,
        IDashboardFilteredReportsScope dashboardFilteredReportsScope)
    {
        _nyssContext = nyssContext;
        _userService = userService;
        _projectService = projectService;
        _config = config;
        _authorizationService = authorizationService;
        _dateTimeProvider = dateTimeProvider;
        _stringsService = stringsService;
        _nationalSocietyStructureService = nationalSocietyStructureService;
        _dhisService = dhisService;
        _loggerAdapter = loggerAdapter;
        _dashboardFilteredReportsScope = dashboardFilteredReportsScope;
    }


    public async Task<Result<PaginatedList<ReportListResponseDto>>> List(
        int projectId,
        int pageNumber,
        ReportListFilterRequestDto filter)
    {
        var currentUserName = _authorizationService.GetCurrentUserName();
        var currentUser = await _authorizationService.GetCurrentUser();

        if (currentUser == null || currentUserName == null)
        {
            return Error<PaginatedList<ReportListResponseDto>>(ResultKey.User.Common.UserNotFound);
        }

        var currentRole = currentUser.Role;
        var currentUserId = currentUser.Id;

        var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(currentUserName);

        var strings = await _stringsService.GetForCurrentUser();
        if (strings == null)
        {
            return Error<PaginatedList<ReportListResponseDto>>(ResultKey.User.Common.UserNotFound);
        }

        var baseQuery = await BuildRawReportsBaseQuery(filter, projectId, currentUser);

        if (baseQuery == null)
        {
            return Error<PaginatedList<ReportListResponseDto>>(ResultKey.User.Common.UserNotFound);
        }

        var currentUserOrganization = await _nyssContext.Projects
            .Where(p => p.Id == projectId)
            .SelectMany(p => p.NationalSociety.NationalSocietyUsers!)
            .Where(uns => uns.User != null && uns.User.Id == currentUserId)
            .Select(uns => uns.Organization)
            .SingleOrDefaultAsync();

        var currentUserOrganizationId = currentUserOrganization?.Id ?? 0;

        var result = baseQuery.Select(r => new ReportListResponseDto
        {
            Id = r.Id,
            IsAnonymized = currentRole != Role.Administrator
                && !r.NationalSociety!.NationalSocietyUsers!.Any(nsu =>
                    (r.DataCollector != null && r.DataCollector.Supervisor != null
                        && nsu.UserId == r.DataCollector.Supervisor.Id
                        && nsu.OrganizationId == currentUserOrganizationId)
                    || (r.DataCollector != null && r.DataCollector.HeadSupervisor != null
                        && nsu.UserId == r.DataCollector.HeadSupervisor.Id
                        && nsu.OrganizationId == currentUserOrganizationId)),
            OrganizationName = r.DataCollector != null
                ? r.NationalSociety!.NationalSocietyUsers!
                    .Where(nsu =>
                        (r.DataCollector.Supervisor != null && nsu.UserId == r.DataCollector.Supervisor.Id)
                        || (r.DataCollector.HeadSupervisor != null && nsu.UserId == r.DataCollector.HeadSupervisor.Id))
                    .Select(nsu => nsu.Organization != null ? nsu.Organization.Name : null)
                    .FirstOrDefault()
                : null,
            DateTime = r.ReceivedAt.AddHours(filter != null ? filter.UtcOffset : 0),
            HealthRiskName = r.Report != null
                && r.Report.ProjectHealthRisk != null
                && r.Report.ProjectHealthRisk.HealthRisk != null
                ? r.Report.ProjectHealthRisk.HealthRisk.LanguageContents
                    .Where(lc => lc.ContentLanguage.LanguageCode == userApplicationLanguageCode)
                    .Select(lc => lc.Name)
                    .FirstOrDefault() ?? string.Empty
                : string.Empty,
            IsActivityReport = r.Report != null
                && r.Report.ProjectHealthRisk != null
                && r.Report.ProjectHealthRisk.HealthRisk != null
                && (r.Report.ProjectHealthRisk.HealthRisk.HealthRiskCode == 99
                    || r.Report.ProjectHealthRisk.HealthRisk.HealthRiskCode == 98),
            IsValid = r.Report != null,
            Region = r.Village != null && r.Village.District != null && r.Village.District.Region != null
                ? r.Village.District.Region.Name
                : string.Empty,
            District = r.Village != null && r.Village.District != null
                ? r.Village.District.Name
                : string.Empty,
            Village = r.Village != null
                ? r.Village.Name ?? string.Empty
                : string.Empty,
            Zone = r.Zone != null ? r.Zone.Name : null,
            DataCollectorDisplayName = r.DataCollector != null
                ? r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint
                    ? r.DataCollector.Name ?? string.Empty
                    : r.DataCollector.DisplayName ?? string.Empty
                : string.Empty,
            SupervisorName = r.DataCollector != null
                ? r.DataCollector.Supervisor != null
                    ? r.DataCollector.Supervisor.Name
                    : r.DataCollector.HeadSupervisor != null
                        ? r.DataCollector.HeadSupervisor.Name
                        : null
                : null,
            PhoneNumber = r.Sender,
            Alert = r.Report != null
                ? r.Report.ReportAlerts
                    .OrderByDescending(ra => ra.AlertId)
                    .Select(ra => new ReportListAlert
                    {
                        Id = ra.AlertId,
                        Status = ra.Alert != null ? ra.Alert.Status : default,
                        ReportWasCrossCheckedBeforeEscalation = ra.Alert != null
                            && ((ra.Report.AcceptedAt.HasValue
                                    && ra.Alert.EscalatedAt.HasValue
                                    && ra.Report.AcceptedAt < ra.Alert.EscalatedAt)
                                || (ra.Report.RejectedAt.HasValue
                                    && ra.Alert.EscalatedAt.HasValue
                                    && ra.Report.RejectedAt < ra.Alert.EscalatedAt))
                    })
                    .FirstOrDefault()
                : null,
            ReportId = r.ReportId,
            ReportType = r.Report != null ? (ReportType?)r.Report.ReportType : null,
            Message = r.Text,
            CountMalesBelowFive = r.Report != null && r.Report.ReportedCase != null
                ? r.Report.ReportedCase.CountMalesBelowFive
                : null,
            CountMalesAtLeastFive = r.Report != null && r.Report.ReportedCase != null
                ? r.Report.ReportedCase.CountMalesAtLeastFive
                : null,
            CountFemalesBelowFive = r.Report != null && r.Report.ReportedCase != null
                ? r.Report.ReportedCase.CountFemalesBelowFive
                : null,
            CountFemalesAtLeastFive = r.Report != null && r.Report.ReportedCase != null
                ? r.Report.ReportedCase.CountFemalesAtLeastFive
                : null,
            ReferredCount = r.Report != null && r.Report.DataCollectionPointCase != null
                ? r.Report.DataCollectionPointCase.ReferredCount
                : null,
            Status = r.Report != null ? r.Report.Status : ReportStatus.New,
            ReportErrorType = r.ErrorType,
            DataCollectorIsDeleted = r.DataCollector != null && r.DataCollector.Name == Anonymization.Text,
            IsCorrected = r.MarkedAsCorrectedAtUtc != null,
        })
        .OrderBy(r => r.DateTime, filter?.SortAscending ?? true);

        var rowsPerPage = _config.PaginationRowsPerPage;
        var reports = await result
            .Page(pageNumber, rowsPerPage)
            .ToListAsync<IReportListResponseDto>();


        if (filter?.DataCollectorType != ReportListDataCollectorType.UnknownSender
            && currentUserOrganization?.Name != null)
        {
            AnonymizeCrossOrganizationReports(reports, currentUserOrganization.Name, strings);
        }

        var totalCount = await baseQuery.CountAsync();
        return Success(reports.Cast<ReportListResponseDto>().AsPaginatedList(pageNumber, totalCount, rowsPerPage));
    }


    public async Task<Result<ReportListFilterResponseDto>> GetFilters(int projectId)
    {
        var healthRiskTypes = new List<HealthRiskType>
        {
            HealthRiskType.Human,
            HealthRiskType.NonHuman,
            HealthRiskType.UnusualEvent,
            HealthRiskType.Activity
        };
        var projectHealthRiskNames = await _projectService.GetHealthRiskNames(projectId, healthRiskTypes);
        var nationalSocietyId = await _nyssContext.Projects
            .Where(p => p.Id == projectId)
            .Select(p => p.NationalSocietyId)
            .SingleAsync();
        var locations = await _nationalSocietyStructureService.Get(nationalSocietyId);

        var dto = new ReportListFilterResponseDto
        {
            HealthRisks = projectHealthRiskNames,
            Locations = locations
        };

        return Success(dto);
    }

    public async Task<Result<HumanHealthRiskResponseDto>> GetHumanHealthRisksForProject(int projectId)
    {
        var humanHealthRiskType = new List<HealthRiskType> { HealthRiskType.Human };
        var projectHealthRisks = await _projectService.GetHealthRiskNames(projectId, humanHealthRiskType);

        var dto = new HumanHealthRiskResponseDto { HealthRisks = projectHealthRisks };

        return Success(dto);
    }

    public IQueryable<RawReport> GetRawReportsWithDataCollectorQuery(ReportsFilter filters) =>
        _nyssContext.RawReports
            .AsNoTracking()
            .FilterByReportStatus(filters.ReportStatus)
            .FromKnownDataCollector()
            .FilterByArea(filters.Area)
            .FilterByDataCollectorType(filters.DataCollectorType)
            .FilterByOrganization(filters.OrganizationId)
            .FilterByProject(filters.ProjectId)
            .FilterReportsByNationalSociety(filters.NationalSocietyId)
            .FilterByDate(filters.StartDate, filters.EndDate)
            .FilterByHealthRisks(filters.HealthRisks)
            .FilterByTrainingMode(filters.TrainingStatus);

    public IQueryable<RawReport> GetRawReportsWithDataCollectorAndActivityReportsQuery(ReportsFilter filters) =>
        _nyssContext.RawReports
            .AsNoTracking()
            .FilterByReportStatus(filters.ReportStatus)
            .FromKnownDataCollector()
            .FilterByArea(filters.Area)
            .FilterByDataCollectorType(filters.DataCollectorType)
            .FilterByOrganization(filters.OrganizationId)
            .FilterByProject(filters.ProjectId)
            .FilterReportsByNationalSociety(filters.NationalSocietyId)
            .FilterByDate(filters.StartDate, filters.EndDate)
            .FilterByHealthRisksWithActivityReports(filters.HealthRisks)
            .FilterByTrainingMode(filters.TrainingStatus);


    public IQueryable<Report> GetDashboardHealthRiskEventReportsQuery(ReportsFilter filters)
    {
        if (_dashboardFilteredReportsScope.IsInitialized)
        {
            return _dashboardFilteredReportsScope.GetDashboardReportsQuery();
        }

        return DashboardReportQuery.ApplyFilters(_nyssContext.Reports.AsNoTracking(), filters);
    }

    public async Task<Result> AcceptReport(int reportId)
    {
        var currentUser = await _authorizationService.GetCurrentUser();
        var report = await _nyssContext.RawReports
            .Where(r => r.Id == reportId && r.Report != null)
            .Select(r => r.Report)
            .FirstOrDefaultAsync();

        if (report == null)
        {
            return Error(ResultKey.Report.ReportNotFound);
        }

        if (report.Status == ReportStatus.Accepted)
        {
            return Error(ResultKey.Report.AlreadyCrossChecked);
        }

        if (report.Location == null)
        {
            return Error(ResultKey.Report.CannotCrossCheckReportWithoutLocation);
        }

        report.AcceptedAt = _dateTimeProvider.UtcNow;
        report.AcceptedBy = currentUser;
        report.Status = ReportStatus.Accepted;

        await _nyssContext.SaveChangesAsync();
        return Success();
    }

    public async Task<Result> DismissReport(int reportId)
    {
        var currentUser = await _authorizationService.GetCurrentUser();
        var report = await _nyssContext.RawReports
            .Where(r => r.Id == reportId && r.Report != null)
            .Select(r => r.Report)
            .FirstOrDefaultAsync();

        if (report == null)
        {
            return Error(ResultKey.Report.ReportNotFound);
        }

        if (report.Status == ReportStatus.Rejected)
        {
            return Error(ResultKey.Report.AlreadyCrossChecked);
        }

        if (report.Location == null)
        {
            return Error(ResultKey.Report.CannotCrossCheckReportWithoutLocation);
        }

        report.RejectedAt = _dateTimeProvider.UtcNow;
        report.RejectedBy = currentUser;
        report.Status = ReportStatus.Rejected;

        await _nyssContext.SaveChangesAsync();
        return Success();
    }

    private async Task<IQueryable<RawReport>> BuildRawReportsBaseQuery(ReportListFilterRequestDto filter, int projectId, User currentUser)
    {
        if (filter.DataCollectorType == ReportListDataCollectorType.UnknownSender)
        {
            var nationalSocietyId = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => p.NationalSocietyId)
                .SingleOrDefaultAsync();

            return _nyssContext.RawReports
                .AsNoTracking()
                .Include(r => r.Report!)
                .ThenInclude(r => r.ProjectHealthRisk!)
                .ThenInclude(r => r.HealthRisk)
                .Where(r => r.NationalSociety != null && r.NationalSociety.Id == nationalSocietyId)
                .FilterByDataCollectorType(filter.DataCollectorType)
                .FilterByHealthRisks(filter.HealthRisks)
                .FilterByErrorType(filter.ErrorType)
                .FilterByArea(filter.Locations)
                .FilterByDate(filter.StartDate, filter.EndDate.AddDays(1))
                .FilterByReportStatus(filter.ReportStatus)
                .FilterByTrainingMode(filter.TrainingStatus)
                .FilterByCorrectedState(filter.CorrectedState);
        }

        return _nyssContext.RawReports
            .AsNoTracking()
            .Include(r => r.Report!)
            .ThenInclude(r => r.ProjectHealthRisk!)
            .ThenInclude(r => r.HealthRisk)
            .FilterByProject(projectId)
            .FilterByHealthRisks(filter.HealthRisks)
            .FilterByDataCollectorType(filter.DataCollectorType)
            .FilterByArea(filter.Locations)
            .FilterByDate(filter.StartDate, filter.EndDate.AddDays(1))
            .FilterByErrorType(filter.ErrorType)
            .FilterByReportStatus(filter.ReportStatus)
            .FilterByTrainingMode(filter.TrainingStatus)
            .FilterByCorrectedState(filter.CorrectedState)
            .FilterByCurrentUserSupervisorAssignment(currentUser);
    }

    internal static void AnonymizeCrossOrganizationReports(
        IEnumerable<IReportListResponseDto> reports,
        string currentUserOrganizationName,
        StringsResourcesVault strings) =>
        reports
            .Where(r => r.IsAnonymized)
            .ToList()
            .ForEach(x =>
            {
                x.DataCollectorDisplayName = x.OrganizationName == currentUserOrganizationName
                    ? $"{strings[ResultKey.Report.LinkedToSupervisor]} {x.SupervisorName ?? string.Empty}".TrimEnd()
                    : $"{strings[ResultKey.Report.LinkedToOrganization]} {x.OrganizationName ?? string.Empty}".TrimEnd();
                x.PhoneNumber = "";
                x.Zone = "";
                x.Village = "";
            });

}

public interface IDashboardFilteredReportsScope
{
    bool IsInitialized { get; }

    Task EnsureInitializedAsync(ReportsFilter filters, CancellationToken cancellationToken = default);

    IQueryable<Report> GetDashboardReportsQuery();
}

public class DashboardFilteredReportsScope : IDashboardFilteredReportsScope
{
    private readonly INyssContext _nyssContext;
    private ReportsFilter _filters = new();
    private List<int> _reportIds = new();

    public DashboardFilteredReportsScope(INyssContext nyssContext)
    {
        _nyssContext = nyssContext;
    }

    public bool IsInitialized { get; private set; }

    public async Task EnsureInitializedAsync(ReportsFilter filters, CancellationToken cancellationToken = default)
    {
        if (IsInitialized && FiltersMatch(_filters, filters))
        {
            return;
        }

        _filters = filters;
        _reportIds = await DashboardReportQuery
            .ApplyFilters(_nyssContext.Reports.AsNoTracking(), filters)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        IsInitialized = true;
    }

    public IQueryable<Report> GetDashboardReportsQuery()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("Dashboard report scope is not initialized.");
        }

        return _nyssContext.Reports
            .AsNoTracking()
            .Where(r => _reportIds.Contains(r.Id));
    }

    private static bool FiltersMatch(ReportsFilter left, ReportsFilter right) =>
        left.NationalSocietyId == right.NationalSocietyId
        && left.ProjectId == right.ProjectId
        && left.OrganizationId == right.OrganizationId
        && left.DataCollectorType == right.DataCollectorType
        && left.StartDate == right.StartDate
        && left.EndDate == right.EndDate
        && left.UtcOffset == right.UtcOffset
        && left.TrainingStatus == right.TrainingStatus
        && left.ReportStatus.Kept == right.ReportStatus.Kept
        && left.ReportStatus.Dismissed == right.ReportStatus.Dismissed
        && left.ReportStatus.NotCrossChecked == right.ReportStatus.NotCrossChecked
        && left.HealthRisks.SequenceEqual(right.HealthRisks)
        && left.Area.IncludeUnknownLocation == right.Area.IncludeUnknownLocation
        && left.Area.RegionIds.SequenceEqual(right.Area.RegionIds)
        && left.Area.DistrictIds.SequenceEqual(right.Area.DistrictIds)
        && left.Area.VillageIds.SequenceEqual(right.Area.VillageIds)
        && left.Area.ZoneIds.SequenceEqual(right.Area.ZoneIds);
}

internal static class DashboardReportQuery
{
    public static IQueryable<Report> ApplyFilters(IQueryable<Report> reports, ReportsFilter filters) =>
        reports
            .Where(r =>
                r.ProjectHealthRisk != null &&
                r.ProjectHealthRisk.HealthRisk != null &&
                r.ProjectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity)
            .FilterByReportStatus(filters.ReportStatus)
            .FilterByKnownDataCollector()
            .FilterByArea(filters.Area)
            .FilterByDataCollectorType(filters.DataCollectorType)
            .FilterByOrganization(filters.OrganizationId)
            .FilterByProject(filters.ProjectId)
            .FilterByNationalSociety(filters.NationalSocietyId)
            .FilterByDate(filters.StartDate, filters.EndDate)
            .FilterByHealthRisks(filters.HealthRisks)
            .FilterByTrainingMode(filters.TrainingStatus);

    public static IQueryable<Report> FilterByReportStatus(this IQueryable<Report> reports, ReportStatusFilterDto filterDto) =>
        filterDto != null && (filterDto.Kept || filterDto.Dismissed || filterDto.NotCrossChecked)
            ? reports.Where(r =>
                (filterDto.Kept && r.Status == ReportStatus.Accepted) ||
                (filterDto.Dismissed && r.Status == ReportStatus.Rejected) ||
                (filterDto.NotCrossChecked && (r.Status == ReportStatus.New || r.Status == ReportStatus.Pending || r.Status == ReportStatus.Closed)))
            : reports;

    public static IQueryable<Report> FilterByKnownDataCollector(this IQueryable<Report> reports) =>
        reports.Where(r => r.DataCollector != null);

    public static IQueryable<Report> FilterByDate(this IQueryable<Report> reports, DateTimeOffset startDate, DateTimeOffset endDate) =>
        reports.Where(r => r.ReceivedAt >= startDate && r.ReceivedAt < endDate);

    public static IQueryable<Report> FilterByHealthRisks(this IQueryable<Report> reports, IList<int> healthRiskIds) =>
        healthRiskIds != null && healthRiskIds.Any()
            ? reports.Where(r => healthRiskIds.Contains(r.ProjectHealthRisk!.HealthRiskId))
            : reports;

    public static IQueryable<Report> FilterByProject(this IQueryable<Report> reports, int? projectId) =>
        projectId.HasValue
            ? reports.Where(r => r.ProjectHealthRisk!.Project.Id == projectId.Value)
            : reports;

    public static IQueryable<Report> FilterByNationalSociety(this IQueryable<Report> reports, int? nationalSocietyId) =>
        nationalSocietyId.HasValue
            ? reports.Where(r => r.ProjectHealthRisk!.Project!.NationalSocietyId == nationalSocietyId.Value)
            : reports;

    public static IQueryable<Report> FilterByDataCollectorType(this IQueryable<Report> reports, DataCollectorType? dataCollectorType) =>
        dataCollectorType switch
        {
            DataCollectorType.Human => reports.Where(r => r.DataCollector!.DataCollectorType == DataCollectorType.Human),
            DataCollectorType.CollectionPoint => reports.Where(r => r.DataCollector!.DataCollectorType == DataCollectorType.CollectionPoint),
            _ => reports
        };

    public static IQueryable<Report> FilterByOrganization(this IQueryable<Report> reports, int? organizationId) =>
        organizationId.HasValue
            ? reports.Where(r => r.DataCollector!.Supervisor!.UserNationalSocieties!
                .Any(uns =>
                    uns.NationalSociety == r.DataCollector!.Project!.NationalSociety
                    && uns.OrganizationId == organizationId.Value))
            : reports;

    public static IQueryable<Report> FilterByArea(this IQueryable<Report> reports, AreaDto area) =>
        area != null && (area.RegionIds.Any() || area.DistrictIds.Any() || area.VillageIds.Any() || area.ZoneIds.Any() || area.IncludeUnknownLocation)
            ? reports.Where(r =>
                (r.RawReport != null &&
                 r.RawReport.Village != null &&
                 area.RegionIds.Contains(r.RawReport.Village.District!.Region.Id) &&
                 area.DistrictIds.Contains(r.RawReport.Village.District.Id) &&
                 area.VillageIds.Contains(r.RawReport.Village.Id) &&
                 (r.RawReport.Zone == null || area.ZoneIds.Contains(r.RawReport.Zone.Id)))
                || (area.IncludeUnknownLocation && (r.RawReport == null || r.RawReport.Village == null)))
            : reports;

    public static IQueryable<Report> FilterByTrainingMode(this IQueryable<Report> reports, TrainingStatusDto? trainingStatus) =>
        trainingStatus switch
        {
            TrainingStatusDto.InTraining => reports.Where(r => r.IsTraining),
            TrainingStatusDto.Trained => reports.Where(r => !r.IsTraining),
            _ => reports
        };
}