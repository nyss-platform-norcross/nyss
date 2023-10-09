using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
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

    public ReportService(
        INyssContext nyssContext,
        IUserService userService,
        IProjectService projectService,
        INyssWebConfig config,
        IAuthorizationService authorizationService,
        IDateTimeProvider dateTimeProvider,
        IStringsService stringsService,
        INationalSocietyStructureService nationalSocietyStructureService)
    {
        _nyssContext = nyssContext;
        _userService = userService;
        _projectService = projectService;
        _config = config;
        _authorizationService = authorizationService;
        _dateTimeProvider = dateTimeProvider;
        _stringsService = stringsService;
        _nationalSocietyStructureService = nationalSocietyStructureService;
    }

    public async Task<Result<PaginatedList<ReportListResponseDto>>> List(int projectId, int pageNumber, ReportListFilterRequestDto filter)
    {
        var currentUserName = _authorizationService.GetCurrentUserName();
        var currentRole = (await _authorizationService.GetCurrentUser()).Role;

        var isSupervisor = currentRole == Role.Supervisor;
        var isHeadSupervisor = currentRole == Role.HeadSupervisor;
        var currentUserId = await _nyssContext.Users.FilterAvailable()
            .Where(u => u.EmailAddress == currentUserName)
            .Select(u => u.Id)
            .SingleAsync();

        var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(currentUserName);
        var strings = await _stringsService.GetForCurrentUser();

        var baseQuery = await BuildRawReportsBaseQuery(filter, projectId);

        var currentUserOrganization = await _nyssContext.Projects
            .Where(p => p.Id == projectId)
            .SelectMany(p => p.NationalSociety.NationalSocietyUsers)
            .Where(uns => uns.User.Id == currentUserId)
            .Select(uns => uns.Organization)
            .SingleOrDefaultAsync();

        var result = baseQuery.Select(r => new ReportListResponseDto
        {
            Id = r.Id,
            IsAnonymized = isSupervisor || isHeadSupervisor
                    ? (currentRole == Role.HeadSupervisor && r.DataCollector.Supervisor.HeadSupervisor.Id != currentUserId)
                    || (currentRole == Role.Supervisor && r.DataCollector.Supervisor.Id != currentUserId)
                    : currentRole != Role.Administrator && !r.NationalSociety.NationalSocietyUsers.Any(
                        nsu => (nsu.UserId == r.DataCollector.Supervisor.Id && nsu.OrganizationId == currentUserOrganization.Id)
                            || (nsu.UserId == r.DataCollector.HeadSupervisor.Id && nsu.OrganizationId == currentUserOrganization.Id)),
            OrganizationName = r.NationalSociety.NationalSocietyUsers
                    .Where(nsu => nsu.UserId == r.DataCollector.Supervisor.Id || nsu.UserId == r.DataCollector.HeadSupervisor.Id)
                    .Select(nsu => nsu.Organization.Name)
                    .FirstOrDefault(),
            DateTime = r.ReceivedAt.AddHours(filter.UtcOffset),
            HealthRiskName = r.Report.ProjectHealthRisk.HealthRisk.LanguageContents
                    .Where(lc => lc.ContentLanguage.LanguageCode == userApplicationLanguageCode)
                    .Select(lc => lc.Name)
                    .SingleOrDefault(),
            IsActivityReport = r.Report.ProjectHealthRisk.HealthRisk.HealthRiskCode == 99
                    || r.Report.ProjectHealthRisk.HealthRisk.HealthRiskCode == 98,
            IsValid = r.Report != null,
            Region = r.Village.District.Region.Name,
            District = r.Village.District.Name,
            Village = r.Village.Name,
            Zone = r.Zone.Name,
            DataCollectorDisplayName = r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint
                    ? r.DataCollector.Name
                    : r.DataCollector.DisplayName,
            SupervisorName = r.DataCollector.Supervisor.Name ?? r.DataCollector.HeadSupervisor.Name,
            PhoneNumber = r.Sender,
            Alert = r.Report.ReportAlerts
                    .OrderByDescending(ra => ra.AlertId)
                    .Select(ra => new ReportListAlert
                    {
                        Id = ra.AlertId,
                        Status = ra.Alert.Status,
                        ReportWasCrossCheckedBeforeEscalation = ra.Report.AcceptedAt < ra.Alert.EscalatedAt || ra.Report.RejectedAt < ra.Alert.EscalatedAt
                    })
                    .FirstOrDefault(),
            ReportId = r.ReportId,
            ReportType = r.Report.ReportType,
            Message = r.Text,
            CountMalesBelowFive = r.Report.ReportedCase.CountMalesBelowFive,
            CountMalesAtLeastFive = r.Report.ReportedCase.CountMalesAtLeastFive,
            CountFemalesBelowFive = r.Report.ReportedCase.CountFemalesBelowFive,
            CountFemalesAtLeastFive = r.Report.ReportedCase.CountFemalesAtLeastFive,
            ReferredCount = r.Report.DataCollectionPointCase.ReferredCount,
            DeathCount = r.Report.DataCollectionPointCase.DeathCount,
            FromOtherVillagesCount = r.Report.DataCollectionPointCase.FromOtherVillagesCount,
            Status = r.Report.Status != null
                    ? r.Report.Status
                    : ReportStatus.New,
            ReportErrorType = r.ErrorType,
            DataCollectorIsDeleted = r.DataCollector != null && r.DataCollector.Name == Anonymization.Text,
            IsCorrected = r.MarkedAsCorrectedAtUtc != null,
        })
            //ToDo: order base on filter.OrderBy property
            .OrderBy(r => r.DateTime, filter.SortAscending);

        var rowsPerPage = _config.PaginationRowsPerPage;
        var reports = await result
            .Page(pageNumber, rowsPerPage)
            .ToListAsync<IReportListResponseDto>();

        if (filter.DataCollectorType != ReportListDataCollectorType.UnknownSender)
        {
            AnonymizeCrossOrganizationReports(reports, currentUserOrganization?.Name, strings);
        }

        return Success(reports.Cast<ReportListResponseDto>().AsPaginatedList(pageNumber, await baseQuery.CountAsync(), rowsPerPage));
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


    public IQueryable<Report> GetDashboardHealthRiskEventReportsQuery(ReportsFilter filters) =>
        GetRawReportsWithDataCollectorQuery(filters)
            .AllSuccessfulReports()
            .Select(r => r.Report)
            .Where(r => r.ProjectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity);

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

        if (report.ReportType == ReportType.DataCollectionPoint)
        {
            return Error(ResultKey.Report.CannotCrossCheckDcpReport);
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

        if (report.ReportType == ReportType.DataCollectionPoint)
        {
            return Error(ResultKey.Report.CannotCrossCheckDcpReport);
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

    private async Task<IQueryable<RawReport>> BuildRawReportsBaseQuery(ReportListFilterRequestDto filter, int projectId)
    {
        if (filter.DataCollectorType == ReportListDataCollectorType.UnknownSender)
        {
            var nationalSocietyId = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => p.NationalSocietyId)
                .SingleOrDefaultAsync();

            return _nyssContext.RawReports
                .AsNoTracking()
                .Include(r => r.Report)
                .ThenInclude(r => r.ProjectHealthRisk)
                .ThenInclude(r => r.HealthRisk)
                .Where(r => r.NationalSociety.Id == nationalSocietyId)
                .FilterByDataCollectorType(filter.DataCollectorType)
                .FilterByHealthRisks(filter.HealthRisks)
                .FilterByErrorType(filter.ErrorType)
                .FilterByArea(filter.Locations)
                .FilterByReportStatus(filter.ReportStatus)
                .FilterByTrainingMode(filter.TrainingStatus)
                .FilterByCorrectedState(filter.CorrectedState);
        }

        return _nyssContext.RawReports
            .AsNoTracking()
            .Include(r => r.Report)
            .ThenInclude(r => r.ProjectHealthRisk)
            .ThenInclude(r => r.HealthRisk)
            .FilterByProject(projectId)
            .FilterByHealthRisks(filter.HealthRisks)
            .FilterByDataCollectorType(filter.DataCollectorType)
            .FilterByArea(filter.Locations)
            .FilterByErrorType(filter.ErrorType)
            .FilterByReportStatus(filter.ReportStatus)
            .FilterByTrainingMode(filter.TrainingStatus)
            .FilterByCorrectedState(filter.CorrectedState);
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
                    ? $"{strings[ResultKey.Report.LinkedToSupervisor]} {x.SupervisorName}"
                    : $"{strings[ResultKey.Report.LinkedToOrganization]} {x.OrganizationName}";
                x.PhoneNumber = "";
                x.Zone = "";
                x.Village = "";
            });
}