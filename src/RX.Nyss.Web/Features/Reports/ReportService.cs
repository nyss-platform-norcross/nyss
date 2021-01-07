using System;
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
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.Projects;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Features.Users;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Reports
{
    public interface IReportService
    {
        Task<Result<ReportResponseDto>> Get(int reportId);
        Task<Result<PaginatedList<ReportListResponseDto>>> List(int projectId, int pageNumber, ReportListFilterRequestDto filter);
        Task<Result<ReportListFilterResponseDto>> GetFilters(int nationalSocietyId);
        Task<Result<HumanHealthRiskResponseDto>> GetHumanHealthRisksForProject(int projectId);
        Task<Result> Edit(int reportId, ReportRequestDto reportRequestDto);
        Task<byte[]> Export(int projectId, ReportListFilterRequestDto filter, bool useExcelFormat = false);
        Task<Result> MarkAsError(int reportId);
        IQueryable<RawReport> GetRawReportsWithDataCollectorQuery(ReportsFilter filters);
        IQueryable<Report> GetHealthRiskEventReportsQuery(ReportsFilter filters);
        IQueryable<Report> GetSuccessReportsQuery(ReportsFilter filters);
    }

    public class ReportService : IReportService
    {
        private readonly INyssWebConfig _config;
        private readonly INyssContext _nyssContext;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IExcelExportService _excelExportService;
        private readonly IStringsResourcesService _stringsResourcesService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ReportService(INyssContext nyssContext, IUserService userService, IProjectService projectService, INyssWebConfig config, IAuthorizationService authorizationService,
            IExcelExportService excelExportService, IStringsResourcesService stringsResourcesService, IDateTimeProvider dateTimeProvider)
        {
            _nyssContext = nyssContext;
            _userService = userService;
            _projectService = projectService;
            _config = config;
            _authorizationService = authorizationService;
            _excelExportService = excelExportService;
            _stringsResourcesService = stringsResourcesService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result<ReportResponseDto>> Get(int reportId)
        {
            var report = await _nyssContext.Reports
                .Select(r => new ReportResponseDto
                {
                    Id = r.Id,
                    ReportType = r.ReportType,
                    Date = r.ReceivedAt.Date,
                    HealthRiskId = r.ProjectHealthRisk.HealthRiskId,
                    CountMalesBelowFive = r.ReportedCase.CountMalesBelowFive.Value,
                    CountMalesAtLeastFive = r.ReportedCase.CountMalesAtLeastFive.Value,
                    CountFemalesBelowFive = r.ReportedCase.CountFemalesBelowFive.Value,
                    CountFemalesAtLeastFive = r.ReportedCase.CountFemalesAtLeastFive.Value,
                    ReferredCount = r.DataCollectionPointCase.ReferredCount.Value,
                    DeathCount = r.DataCollectionPointCase.DeathCount.Value,
                    FromOtherVillagesCount = r.DataCollectionPointCase.FromOtherVillagesCount.Value
                })
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
            {
                return Error<ReportResponseDto>(ResultKey.Report.ReportNotFound);
            }

            var result = Success(report);

            return result;
        }

        public async Task<Result<PaginatedList<ReportListResponseDto>>> List(int projectId, int pageNumber, ReportListFilterRequestDto filter)
        {
            var currentUserName = _authorizationService.GetCurrentUserName();
            var isSupervisor = _authorizationService.IsCurrentUserInRole(Role.Supervisor);
            var currentUserId = await _nyssContext.Users.FilterAvailable()
                .Where(u => u.EmailAddress == currentUserName)
                .Select(u => u.Id)
                .SingleAsync();

            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(currentUserName);
            var stringResources = (await _stringsResourcesService.GetStringsResources(userApplicationLanguageCode)).Value;

            var baseQuery = _nyssContext.RawReports
                .FilterByProject(projectId)
                .FilterByHealthRisk(filter.HealthRiskId)
                .FilterByTrainingMode(filter.IsTraining)
                .FilterByDataCollectorType(filter.ReportsType == ReportListType.FromDcp
                    ? DataCollectorType.CollectionPoint
                    : DataCollectorType.Human)
                .FilterByArea(MapToArea(filter.Area))
                .Where(r => filter.Status
                    ? r.Report != null && !r.Report.MarkedAsError
                    : r.Report == null || r.Report.MarkedAsError);


            var currentUserOrganization = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.NationalSociety.NationalSocietyUsers)
                .Where(uns => uns.User.Id == currentUserId)
                .Select(uns => uns.Organization)
                .SingleOrDefaultAsync();

            var currentRole = (await _authorizationService.GetCurrentUser()).Role;

            var result = baseQuery.Select(r => new ReportListResponseDto
                {
                    Id = r.Id,
                    IsAnonymized = isSupervisor
                        ? r.DataCollector.Supervisor.Id != currentUserId
                        : currentRole != Role.Administrator && !r.NationalSociety.NationalSocietyUsers.Any(
                            nsu => nsu.UserId == r.DataCollector.Supervisor.Id && nsu.OrganizationId == currentUserOrganization.Id),
                    OrganizationName = r.NationalSociety.NationalSocietyUsers
                        .Where(nsu => nsu.UserId == r.DataCollector.Supervisor.Id)
                        .Select(nsu => nsu.Organization.Name)
                        .FirstOrDefault(),
                    DateTime = r.ReceivedAt,
                    HealthRiskName = r.Report.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.LanguageCode == userApplicationLanguageCode)
                        .Select(lc => lc.Name)
                        .Single(),
                    IsValid = r.Report != null,
                    Region = r.Village.District.Region.Name,
                    District = r.Village.District.Name,
                    Village = r.Village.Name,
                    Zone = r.Zone.Name,
                    DataCollectorDisplayName = r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint
                        ? r.DataCollector.Name
                        : r.DataCollector.DisplayName,
                    SupervisorName = r.DataCollector.Supervisor.Name,
                    PhoneNumber = r.Sender,
                    IsMarkedAsError = r.Report.MarkedAsError,
                    UserHasAccessToReportDataCollector = !isSupervisor || r.DataCollector.Supervisor.Id == currentUserId,
                    AlertId = r.Report.ReportAlerts
                        .Select(ra => ra.Alert)
                        .OrderBy(a => a.Status == AlertStatus.Pending ? 0 :
                            a.Status == AlertStatus.Escalated ? 1 : 2)
                        .Select(a => a.Id)
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
                    FromOtherVillagesCount = r.Report.DataCollectionPointCase.FromOtherVillagesCount
                })
                //ToDo: order base on filter.OrderBy property
                .OrderBy(r => r.DateTime, filter.SortAscending);

            var rowsPerPage = _config.PaginationRowsPerPage;
            var reports = await result
                .Page(pageNumber, rowsPerPage)
                .ToListAsync<IReportListResponseDto>();

            await UpdateTimeZoneInReports(projectId, reports);
            AnonymizeCrossOrganizationReports(reports, currentUserOrganization?.Name, stringResources);

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

            var dto = new ReportListFilterResponseDto { HealthRisks = projectHealthRiskNames };

            return Success(dto);
        }

        public async Task<Result<HumanHealthRiskResponseDto>> GetHumanHealthRisksForProject(int projectId)
        {
            var humanHealthRiskType = new List<HealthRiskType> { HealthRiskType.Human };
            var projectHealthRisks = await _projectService.GetHealthRiskNames(projectId, humanHealthRiskType);

            var dto = new HumanHealthRiskResponseDto { HealthRisks = projectHealthRisks };

            return Success(dto);
        }

        public async Task<byte[]> Export(int projectId, ReportListFilterRequestDto filter, bool useExcelFormat = false)
        {
            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(_authorizationService.GetCurrentUserName());
            var stringResources = (await _stringsResourcesService.GetStringsResources(userApplicationLanguageCode)).Value;
            var currentRole = (await _authorizationService.GetCurrentUser()).Role;

            var currentUser = await _authorizationService.GetCurrentUser();

            var currentUserOrganization = await _nyssContext
                .Projects
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.NationalSociety.NationalSocietyUsers)
                .Where(uns => uns.User.Id == currentUser.Id)
                .Select(uns => uns.Organization)
                .SingleOrDefaultAsync();

            var reportsQuery = _nyssContext.RawReports
                .FilterByProject(projectId)
                .FilterByHealthRisk(filter.HealthRiskId)
                .FilterByTrainingMode(filter.IsTraining)
                .FilterByDataCollectorType(filter.ReportsType == ReportListType.FromDcp
                    ? DataCollectorType.CollectionPoint
                    : DataCollectorType.Human)
                .FilterByArea(MapToArea(filter.Area))
                .Where(r => filter.Status
                    ? r.Report != null && !r.Report.MarkedAsError
                    : r.Report == null || (r.Report != null && r.Report.MarkedAsError))
                .Select(r => new ExportReportListResponseDto
                {
                    Id = r.Id,
                    DateTime = r.ReceivedAt,
                    HealthRiskName = r.Report.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.LanguageCode == userApplicationLanguageCode)
                        .Select(lc => lc.Name)
                        .Single(),
                    IsValid = r.Report != null,
                    IsAnonymized = currentRole == Role.Supervisor
                        ? r.DataCollector.Supervisor.Id != currentUser.Id
                        : currentRole != Role.Administrator && !r.NationalSociety.NationalSocietyUsers.Any(
                            nsu => nsu.UserId == r.DataCollector.Supervisor.Id && nsu.OrganizationId == currentUserOrganization.Id),
                    OrganizationName = r.NationalSociety.NationalSocietyUsers
                        .Where(nsu => nsu.UserId == r.DataCollector.Supervisor.Id)
                        .Select(nsu => nsu.Organization.Name)
                        .FirstOrDefault(),
                    SupervisorName = r.DataCollector.Supervisor.Name,
                    Status = GetReportStatus(r.Report != null, r.Report.MarkedAsError, stringResources),
                    MarkedAsError = r.Report.MarkedAsError,
                    Region = r.Village.District.Region.Name,
                    District = r.Village.District.Name,
                    Village = r.Village.Name,
                    Zone = r.Zone.Name,
                    Location = r.Report != null
                        ? r.Report.Location
                        : null,
                    DataCollectorDisplayName = r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint
                        ? r.DataCollector.Name
                        : r.DataCollector.DisplayName,
                    PhoneNumber = r.Sender,
                    Message = r.Text,
                    CountMalesBelowFive = r.Report.ReportedCase.CountMalesBelowFive,
                    CountMalesAtLeastFive = r.Report.ReportedCase.CountMalesAtLeastFive,
                    CountFemalesBelowFive = r.Report.ReportedCase.CountFemalesBelowFive,
                    CountFemalesAtLeastFive = r.Report.ReportedCase.CountFemalesAtLeastFive,
                    ReferredCount = r.Report.DataCollectionPointCase.ReferredCount,
                    DeathCount = r.Report.DataCollectionPointCase.DeathCount,
                    FromOtherVillagesCount = r.Report.DataCollectionPointCase.FromOtherVillagesCount,
                    EpiWeek = r.Report.EpiWeek,
                    EpiYear = r.Report.EpiYear
                })
                //ToDo: order base on filter.OrderBy property
                .OrderBy(r => r.DateTime, filter.SortAscending);

            var reports = await reportsQuery.ToListAsync<IReportListResponseDto>();

            await UpdateTimeZoneInReports(projectId, reports);
            AnonymizeCrossOrganizationReports(reports, currentUserOrganization?.Name, stringResources);

            if (useExcelFormat)
            {
                return GetExcelData(reports, stringResources, filter.ReportsType);
            }

            return GetCsvData(reports, stringResources, filter.ReportsType);
        }

        public async Task<Result> Edit(int reportId, ReportRequestDto reportRequestDto)
        {
            var report = await _nyssContext.Reports
                .Include(r => r.RawReport)
                .Include(r => r.ProjectHealthRisk)
                .ThenInclude(phr => phr.Project)
                .SingleOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
            {
                return Error<ReportResponseDto>(ResultKey.Report.ReportNotFound);
            }

            if (report.ReportType != ReportType.Aggregate &&
                report.ReportType != ReportType.DataCollectionPoint)
            {
                return Error<ReportResponseDto>(ResultKey.Report.Edit.HealthRiskCannotBeEdited);
            }

            if (!await HasAccessToReport(reportId))
            {
                return Error(ResultKey.Report.NoAccess);
            }

            var projectHealthRisk = await _nyssContext.ProjectHealthRisks
                .Include(phr => phr.HealthRisk)
                .SingleOrDefaultAsync(phr => phr.HealthRiskId == reportRequestDto.HealthRiskId &&
                    phr.HealthRisk.HealthRiskType == HealthRiskType.Human &&
                    phr.Project.Id == report.ProjectHealthRisk.Project.Id);

            if (projectHealthRisk == null)
            {
                return Error<ReportResponseDto>(ResultKey.Report.Edit.HealthRiskNotAssignedToProject);
            }

            var updatedReceivedAt = new DateTime(reportRequestDto.Date.Year, reportRequestDto.Date.Month, reportRequestDto.Date.Day,
                report.ReceivedAt.Hour, report.ReceivedAt.Minute, report.ReceivedAt.Second);
            report.RawReport.ReceivedAt = updatedReceivedAt;
            report.ReceivedAt = updatedReceivedAt;
            report.ProjectHealthRisk = projectHealthRisk;
            report.ReportedCase.CountMalesBelowFive = reportRequestDto.CountMalesBelowFive;
            report.ReportedCase.CountMalesAtLeastFive = reportRequestDto.CountMalesAtLeastFive;
            report.ReportedCase.CountFemalesBelowFive = reportRequestDto.CountFemalesBelowFive;
            report.ReportedCase.CountFemalesAtLeastFive = reportRequestDto.CountFemalesAtLeastFive;

            report.ReportedCaseCount = reportRequestDto.CountMalesBelowFive +
                reportRequestDto.CountMalesAtLeastFive +
                reportRequestDto.CountFemalesBelowFive +
                reportRequestDto.CountFemalesAtLeastFive;

            if (report.ReportType == ReportType.DataCollectionPoint)
            {
                report.DataCollectionPointCase.ReferredCount = reportRequestDto.ReferredCount;
                report.DataCollectionPointCase.DeathCount = reportRequestDto.DeathCount;
                report.DataCollectionPointCase.FromOtherVillagesCount = reportRequestDto.FromOtherVillagesCount;
            }

            report.ModifiedAt = _dateTimeProvider.UtcNow;
            report.ModifiedBy = _authorizationService.GetCurrentUserName();

            await _nyssContext.SaveChangesAsync();

            return SuccessMessage(ResultKey.Report.Edit.EditSuccess);
        }

        public IQueryable<RawReport> GetRawReportsWithDataCollectorQuery(ReportsFilter filters) =>
            _nyssContext.RawReports
                .FilterByTrainingMode(filters.IsTraining)
                .FromKnownDataCollector()
                .FilterByArea(filters.Area)
                .FilterByDataCollectorType(filters.DataCollectorType)
                .FilterByOrganization(filters.OrganizationId)
                .FilterByProject(filters.ProjectId)
                .FilterReportsByNationalSociety(filters.NationalSocietyId)
                .FilterByDate(filters.StartDate.Date, filters.EndDate.Date)
                .FilterByHealthRisk(filters.HealthRiskId);

        public IQueryable<Report> GetSuccessReportsQuery(ReportsFilter filters) =>
            GetRawReportsWithDataCollectorQuery(filters)
                .AllSuccessfulReports()
                .Select(r => r.Report)
                .Where(r => !r.MarkedAsError);

        public IQueryable<Report> GetHealthRiskEventReportsQuery(ReportsFilter filters) =>
            GetSuccessReportsQuery(filters)
                .Where(r => r.ProjectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity);

        public async Task<Result> MarkAsError(int reportId)
        {
            var report = await _nyssContext.Reports
                .Where(r => !r.ReportAlerts.Any())
                .Include(r => r.ProjectHealthRisk.Project)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (!await HasAccessToReport(reportId))
            {
                return Error(ResultKey.Report.NoAccess);
            }

            if (report.ProjectHealthRisk.Project.State != ProjectState.Open)
            {
                return Error(ResultKey.Report.ProjectIsClosed);
            }

            report.MarkedAsError = true;
            await _nyssContext.SaveChangesAsync();

            return Success();
        }

        private async Task<bool> HasAccessToReport(int reportId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();

            var currentUserOrganizationId = await _nyssContext.Reports
                .Where(p => p.Id == reportId)
                .SelectMany(p => p.ProjectHealthRisk.Project.NationalSociety.NationalSocietyUsers)
                .Where(uns => uns.User == currentUser)
                .Select(uns => uns.OrganizationId)
                .SingleOrDefaultAsync();

            return currentUser.Role == Role.Administrator || _nyssContext.Reports.Any(r => r.ProjectHealthRisk.Project.NationalSociety.NationalSocietyUsers.Any(
                nsu => nsu.UserId == r.DataCollector.Supervisor.Id && nsu.OrganizationId == currentUserOrganizationId));
        }

        private byte[] GetExcelData(List<IReportListResponseDto> reports, IDictionary<string, StringResourceValue> stringResources, ReportListType reportListType)
        {
            var documentTitle = GetStringResource(stringResources, "reports.export.title");
            var columnLabels = GetColumnLabels(stringResources, reportListType);
            var excelDoc = _excelExportService.ToExcel(reports, columnLabels, documentTitle, reportListType);
            return excelDoc.GetAsByteArray();
        }

        private byte[] GetCsvData(List<IReportListResponseDto> reports, IDictionary<string, StringResourceValue> stringResources, ReportListType reportListType)
        {
            var columnLabels = GetColumnLabels(stringResources, reportListType);

            var reportData = reportListType == ReportListType.FromDcp
                ? reports.Select(r =>
                {
                    var report = (ExportReportListResponseDto)r;
                    return new ExportReportListCsvContentDto
                    {
                        Date = r.DateTime.ToString("yyyy-MM-dd"),
                        Time = report.DateTime.ToString("HH:mm"),
                        Status = report.Status,
                        DataCollectorDisplayName = report.DataCollectorDisplayName,
                        PhoneNumber = report.PhoneNumber,
                        Region = report.Region,
                        District = report.District,
                        Village = report.Village,
                        Zone = report.Zone,
                        HealthRiskName = report.HealthRiskName,
                        CountMalesBelowFive = report.CountMalesBelowFive,
                        CountMalesAtLeastFive = report.CountMalesAtLeastFive,
                        CountFemalesBelowFive = report.CountFemalesBelowFive,
                        CountFemalesAtLeastFive = report.CountFemalesAtLeastFive,
                        TotalBelowFive = report.CountFemalesBelowFive + report.CountMalesBelowFive,
                        TotalAtLeastFive = report.CountMalesAtLeastFive + report.CountFemalesAtLeastFive,
                        TotalMale = report.CountMalesAtLeastFive + report.CountMalesBelowFive,
                        TotalFemale = report.CountFemalesAtLeastFive + report.CountFemalesBelowFive,
                        Total = report.CountMalesBelowFive + report.CountMalesAtLeastFive + report.CountFemalesBelowFive + report.CountFemalesAtLeastFive,
                        ReferredCount = report.ReferredCount,
                        DeathCount = report.DeathCount,
                        FromOtherVillagesCount = report.FromOtherVillagesCount,
                        Location = report.Location != null
                            ? $"{report.Location.Y}/{report.Location.X}"
                            : "",
                        Message = report.Message,
                        EpiWeek = report.EpiYear,
                        EpiYear = report.EpiWeek
                    };
                })
                : reports.Select(r =>
                {
                    var report = (ExportReportListResponseDto)r;
                    return new ExportReportListCsvContentDto
                    {
                        Date = r.DateTime.ToString("yyyy-MM-dd"),
                        Time = report.DateTime.ToString("HH:mm"),
                        Status = report.Status,
                        DataCollectorDisplayName = report.DataCollectorDisplayName,
                        PhoneNumber = report.PhoneNumber,
                        Region = report.Region,
                        District = report.District,
                        Village = report.Village,
                        Zone = report.Zone,
                        HealthRiskName = report.HealthRiskName,
                        CountMalesBelowFive = report.CountMalesBelowFive,
                        CountMalesAtLeastFive = report.CountMalesAtLeastFive,
                        CountFemalesBelowFive = report.CountFemalesBelowFive,
                        CountFemalesAtLeastFive = report.CountFemalesAtLeastFive,
                        TotalBelowFive = report.CountFemalesBelowFive + report.CountMalesBelowFive,
                        TotalAtLeastFive = report.CountMalesAtLeastFive + report.CountFemalesAtLeastFive,
                        TotalMale = report.CountMalesAtLeastFive + report.CountMalesBelowFive,
                        TotalFemale = report.CountFemalesAtLeastFive + report.CountFemalesBelowFive,
                        Total = report.CountMalesBelowFive + report.CountMalesAtLeastFive + report.CountFemalesBelowFive + report.CountFemalesAtLeastFive,
                        Location = report.Location != null
                            ? $"{report.Location.Y}/{report.Location.X}"
                            : "",
                        Message = report.Message,
                        EpiWeek = report.EpiYear,
                        EpiYear = report.EpiWeek
                    };
                });

            return _excelExportService.ToCsv(reportData, columnLabels);
        }

        private static string GetReportStatus(bool isValid, bool markedAsError, IDictionary<string, StringResourceValue> stringResources) =>
            markedAsError switch
            {
                true => GetStringResource(stringResources, "reports.list.markedAsError"),
                false => isValid
                    ? GetStringResource(stringResources, "reports.list.success")
                    : GetStringResource(stringResources, "reports.list.error")
            };

        private List<string> GetColumnLabels(IDictionary<string, StringResourceValue> stringResources, ReportListType reportListType) =>
            reportListType == ReportListType.FromDcp
                ? new List<string>
                {
                    GetStringResource(stringResources, "reports.export.date"),
                    GetStringResource(stringResources, "reports.export.time"),
                    GetStringResource(stringResources, "reports.list.status"),
                    GetStringResource(stringResources, "reports.list.dataCollectorDisplayName"),
                    GetStringResource(stringResources, "reports.list.dataCollectorPhoneNumber"),
                    GetStringResource(stringResources, "reports.list.region"),
                    GetStringResource(stringResources, "reports.list.district"),
                    GetStringResource(stringResources, "reports.list.village"),
                    GetStringResource(stringResources, "reports.list.zone"),
                    GetStringResource(stringResources, "reports.list.healthRisk"),
                    GetStringResource(stringResources, "reports.list.malesBelowFive"),
                    GetStringResource(stringResources, "reports.list.malesAtLeastFive"),
                    GetStringResource(stringResources, "reports.list.femalesBelowFive"),
                    GetStringResource(stringResources, "reports.list.femalesAtLeastFive"),
                    GetStringResource(stringResources, "reports.export.totalBelowFive"),
                    GetStringResource(stringResources, "reports.export.totalAtLeastFive"),
                    GetStringResource(stringResources, "reports.export.totalMale"),
                    GetStringResource(stringResources, "reports.export.totalFemale"),
                    GetStringResource(stringResources, "reports.export.total"),
                    GetStringResource(stringResources, "reports.export.referredCount"),
                    GetStringResource(stringResources, "reports.export.deathCount"),
                    GetStringResource(stringResources, "reports.export.fromOtherVillagesCount"),
                    GetStringResource(stringResources, "reports.export.location"),
                    GetStringResource(stringResources, "reports.export.message"),
                    GetStringResource(stringResources, "reports.export.epiYear"),
                    GetStringResource(stringResources, "reports.export.epiWeek")
                }
                : new List<string>
                {
                    GetStringResource(stringResources, "reports.export.date"),
                    GetStringResource(stringResources, "reports.export.time"),
                    GetStringResource(stringResources, "reports.list.status"),
                    GetStringResource(stringResources, "reports.list.dataCollectorDisplayName"),
                    GetStringResource(stringResources, "reports.list.dataCollectorPhoneNumber"),
                    GetStringResource(stringResources, "reports.list.region"),
                    GetStringResource(stringResources, "reports.list.district"),
                    GetStringResource(stringResources, "reports.list.village"),
                    GetStringResource(stringResources, "reports.list.zone"),
                    GetStringResource(stringResources, "reports.list.healthRisk"),
                    GetStringResource(stringResources, "reports.list.malesBelowFive"),
                    GetStringResource(stringResources, "reports.list.malesAtLeastFive"),
                    GetStringResource(stringResources, "reports.list.femalesBelowFive"),
                    GetStringResource(stringResources, "reports.list.femalesAtLeastFive"),
                    GetStringResource(stringResources, "reports.export.totalBelowFive"),
                    GetStringResource(stringResources, "reports.export.totalAtLeastFive"),
                    GetStringResource(stringResources, "reports.export.totalMale"),
                    GetStringResource(stringResources, "reports.export.totalFemale"),
                    GetStringResource(stringResources, "reports.export.total"),
                    GetStringResource(stringResources, "reports.export.location"),
                    GetStringResource(stringResources, "reports.export.message"),
                    GetStringResource(stringResources, "reports.export.epiYear"),
                    GetStringResource(stringResources, "reports.export.epiWeek")
                };

        private static string GetStringResource(IDictionary<string, StringResourceValue> stringResources, string key) =>
            stringResources.Keys.Contains(key)
                ? stringResources[key].Value
                : key;

        private async Task UpdateTimeZoneInReports(int projectId, List<IReportListResponseDto> reports)
        {
            var project = await _nyssContext.Projects.FindAsync(projectId);
            var projectTimeZone = TimeZoneInfo.FindSystemTimeZoneById(project.TimeZone);
            reports.ForEach(x => x.DateTime = TimeZoneInfo.ConvertTimeFromUtc(x.DateTime, projectTimeZone));
        }

        private static void AnonymizeCrossOrganizationReports(IEnumerable<IReportListResponseDto> reports, string currentUserOrganizationName, IDictionary<string, StringResourceValue> stringsResources) =>
            reports
                .Where(r => r.IsAnonymized)
                .ToList()
                .ForEach(x =>
                {
                    x.DataCollectorDisplayName = x.OrganizationName == currentUserOrganizationName
                        ? $"{GetStringResource(stringsResources, ResultKey.Report.LinkedToSupervisor)} {x.SupervisorName}"
                        : $"{GetStringResource(stringsResources, ResultKey.Report.LinkedToOrganization)} {x.OrganizationName}";
                    x.PhoneNumber = "***";
                    x.Zone = "";
                    x.Village = "";
                });

        private static Area MapToArea(AreaDto area) =>
            area == null
                ? null
                : new Area
                {
                    AreaType = area.Type,
                    AreaId = area.Id
                };
    }
}
