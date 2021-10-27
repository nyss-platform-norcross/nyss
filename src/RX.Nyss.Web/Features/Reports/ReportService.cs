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
using RX.Nyss.Web.Features.Alerts;
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

        Task<Result> MarkAsError(int reportId);

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

        private readonly IAlertReportService _alertReportService;

        private readonly IStringsService _stringsService;

        public ReportService(
            INyssContext nyssContext,
            IUserService userService,
            IProjectService projectService,
            INyssWebConfig config,
            IAuthorizationService authorizationService,
            IDateTimeProvider dateTimeProvider,
            IAlertReportService alertReportService,
            IStringsService stringsService)
        {
            _nyssContext = nyssContext;
            _userService = userService;
            _projectService = projectService;
            _config = config;
            _authorizationService = authorizationService;
            _dateTimeProvider = dateTimeProvider;
            _alertReportService = alertReportService;
            _stringsService = stringsService;
        }

        public async Task<Result<ReportResponseDto>> Get(int reportId)
        {
            var report = await _nyssContext.RawReports
                .Include(r => r.Report)
                .ThenInclude(r => r.ProjectHealthRisk)
                .ThenInclude(r => r.HealthRisk)
                .Select(r => new ReportResponseDto
                {
                    Id = r.Id,
                    DataCollectorId = r.DataCollector.Id,
                    ReportType = r.Report.ReportType,
                    ReportStatus = r.Report.Status,
                    LocationId = r.DataCollector.DataCollectorLocations
                        .Where(dcl => dcl.Village == r.Village && (dcl.Zone == null || dcl.Zone == r.Zone))
                        .Select(dcl => dcl.Id)
                        .FirstOrDefault(),
                    Date = r.ReceivedAt.Date,
                    HealthRiskId = r.Report.ProjectHealthRisk.HealthRiskId,
                    CountMalesBelowFive = r.Report.ReportedCase.CountMalesBelowFive.Value,
                    CountMalesAtLeastFive = r.Report.ReportedCase.CountMalesAtLeastFive.Value,
                    CountFemalesBelowFive = r.Report.ReportedCase.CountFemalesBelowFive.Value,
                    CountFemalesAtLeastFive = r.Report.ReportedCase.CountFemalesAtLeastFive.Value,
                    CountUnspecifiedSexAndAge = r.Report.ReportedCase.CountUnspecifiedSexAndAge.Value,
                    ReferredCount = r.Report.DataCollectionPointCase.ReferredCount,
                    DeathCount = r.Report.DataCollectionPointCase.DeathCount,
                    FromOtherVillagesCount = r.Report.DataCollectionPointCase.FromOtherVillagesCount,
                    IsActivityReport = r.Report.IsActivityReport(),
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
                    IsAnonymized = r.DataCollector != null && (isSupervisor || isHeadSupervisor
                        ? (currentRole == Role.HeadSupervisor && r.DataCollector.Supervisor.HeadSupervisor.Id != currentUserId)
                        || (currentRole == Role.Supervisor && r.DataCollector.Supervisor.Id != currentUserId)
                        : currentRole != Role.Administrator && !r.NationalSociety.NationalSocietyUsers.Any(
                            nsu => nsu.UserId == r.DataCollector.Supervisor.Id && nsu.OrganizationId == currentUserOrganization.Id)),
                    OrganizationName = r.NationalSociety.NationalSocietyUsers
                        .Where(nsu => nsu.UserId == r.DataCollector.Supervisor.Id)
                        .Select(nsu => nsu.Organization.Name)
                        .FirstOrDefault(),
                    DateTime = r.ReceivedAt.AddHours(filter.UtcOffset),
                    HealthRiskName = r.Report.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.LanguageCode == userApplicationLanguageCode)
                        .Select(lc => lc.Name)
                        .SingleOrDefault(),
                    IsActivityReport = r.Report.ProjectHealthRisk != null && (r.Report.ProjectHealthRisk.HealthRisk.HealthRiskCode == 99
                        || r.Report.ProjectHealthRisk.HealthRisk.HealthRiskCode == 98),
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
                    DataCollectorIsDeleted = r.DataCollector != null && r.DataCollector.Name == Anonymization.Text
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

        public async Task<Result> Edit(int reportId, ReportRequestDto reportRequestDto)
        {
            var locationChanged = false;

            try
            {
                var rawReport = await _nyssContext.RawReports
                    .Include(raw => raw.Zone)
                    .Include(r => r.Report)
                    .ThenInclude(r => r.ProjectHealthRisk)
                    .Include(r => r.DataCollector).ThenInclude(phr => phr.Project)
                    .Include(r => r.DataCollector).ThenInclude(dc => dc.DataCollectorLocations).ThenInclude(dcl => dcl.Village)
                    .Include(r => r.DataCollector).ThenInclude(dc => dc.DataCollectorLocations).ThenInclude(dcl => dcl.Zone)
                    .SingleOrDefaultAsync(r => r.Id == reportId);

                if (rawReport?.Report == null)
                {
                    return Error<ReportResponseDto>(ResultKey.Report.ReportNotFound);
                }

                if (rawReport.Report.Status != ReportStatus.New)
                {
                    return Error<ReportResponseDto>(ResultKey.Report.Edit.OnlyNewReportsEditable);
                }

                if (rawReport.Report.ProjectHealthRisk.Id != reportRequestDto.HealthRiskId)
                {
                    await SetProjectHealthRisk(rawReport.Report, reportRequestDto.HealthRiskId, reportRequestDto.DataCollectorId);
                }

                if (rawReport.DataCollector == null || rawReport.DataCollector.Id != reportRequestDto.DataCollectorId)
                {
                    await UpdateDataCollectorForReport(rawReport.Report, reportRequestDto.DataCollectorId);
                }

                var dataCollectorLocations = rawReport.DataCollector != null
                    ? rawReport.DataCollector.DataCollectorLocations.ToList()
                    : new List<DataCollectorLocation>();

                if (LocationNeedsUpdate(rawReport.Report, reportRequestDto, dataCollectorLocations))
                {
                    await UpdateLocationForReport(rawReport.Report, reportRequestDto.DataCollectorLocationId, reportRequestDto.DataCollectorId);
                    locationChanged = true;
                }

                UpdateReportedCaseCountForReport(rawReport.Report, reportRequestDto);

                var updatedReceivedAt = new DateTime(reportRequestDto.Date.Year, reportRequestDto.Date.Month, reportRequestDto.Date.Day,
                    rawReport.ReceivedAt.Hour, rawReport.ReceivedAt.Minute, rawReport.ReceivedAt.Second);

                rawReport.ReceivedAt = updatedReceivedAt;
                rawReport.Report.ReceivedAt = updatedReceivedAt;

                if (rawReport.Report.ReportType != ReportType.DataCollectionPoint &&
                    rawReport.Report.ReportType != ReportType.Aggregate)
                {
                    rawReport.Report.Status = reportRequestDto.ReportStatus;
                }

                rawReport.Report.ModifiedAt = _dateTimeProvider.UtcNow;
                rawReport.Report.ModifiedBy = _authorizationService.GetCurrentUserName();

                await _nyssContext.SaveChangesAsync();

                if (locationChanged && rawReport.Report.Status != ReportStatus.Rejected)
                {
                    await _alertReportService.RecalculateAlertForReport(rawReport.Id);
                }

                return SuccessMessage(ResultKey.Report.Edit.EditSuccess);
            }
            catch (ResultException e)
            {
                return Error(e.Result.Message.Key);
            }
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
                .FilterByHealthRisk(filters.HealthRiskId)
                .FilterByTrainingMode(filters.TrainingStatus);


        public IQueryable<Report> GetDashboardHealthRiskEventReportsQuery(ReportsFilter filters) =>
            GetRawReportsWithDataCollectorQuery(filters)
                .AllSuccessfulReports()
                .Select(r => r.Report)
                .Where(r => !r.MarkedAsError)
                .Where(r => r.ProjectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity);

        public async Task<Result> MarkAsError(int reportId)
        {
            var rawReport = await _nyssContext.RawReports
                .Include(r => r.Report)
                .Include(r => r.DataCollector.Project)
                .Where(r => r.Report != null && !r.Report.ReportAlerts.Any())
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (rawReport.DataCollector.Project.State != ProjectState.Open)
            {
                return Error(ResultKey.Report.ProjectIsClosed);
            }

            rawReport.Report.MarkedAsError = true;
            await _nyssContext.SaveChangesAsync();

            return Success();
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

            if (report.MarkedAsError)
            {
                return Error(ResultKey.Report.CannotCrossCheckErrorReport);
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

            if (report.MarkedAsError)
            {
                return Error(ResultKey.Report.CannotCrossCheckErrorReport);
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
                    .FilterByHealthRisk(filter.HealthRiskId)
                    .FilterByFormatCorrectness(filter.FormatCorrect)
                    .FilterByErrorType(filter.ErrorType)
                    .FilterByArea(MapToArea(filter.Area))
                    .FilterByReportStatus(filter.ReportStatus)
                    .FilterByTrainingMode(filter.TrainingStatus);
            }

            return _nyssContext.RawReports
                .AsNoTracking()
                .Include(r => r.Report)
                .ThenInclude(r => r.ProjectHealthRisk)
                .ThenInclude(r => r.HealthRisk)
                .FilterByProject(projectId)
                .FilterByHealthRisk(filter.HealthRiskId)
                .FilterByDataCollectorType(filter.DataCollectorType)
                .FilterByArea(MapToArea(filter.Area))
                .FilterByFormatCorrectness(filter.FormatCorrect)
                .FilterByErrorType(filter.ErrorType)
                .FilterByReportStatus(filter.ReportStatus)
                .FilterByTrainingMode(filter.TrainingStatus);
        }

        private async Task SetProjectHealthRisk(Report report, int projectHealthRiskId, int dataCollectorId)
        {
            var projectId = report.DataCollector?.Project.Id ?? await _nyssContext.DataCollectors
                .Where(dc => dc.Id == dataCollectorId)
                .Select(dc => dc.Project.Id)
                .SingleOrDefaultAsync();

            var projectHealthRisk = await _nyssContext.ProjectHealthRisks
                .Include(phr => phr.HealthRisk)
                .SingleOrDefaultAsync(phr => phr.HealthRiskId == projectHealthRiskId
                    && phr.Project.Id == projectId);

            if (projectHealthRisk == null)
            {
                throw new ResultException(ResultKey.Report.Edit.HealthRiskNotAssignedToProject);
            }

            report.ProjectHealthRisk = projectHealthRisk;
        }

        private static bool LocationNeedsUpdate(Report report, ReportRequestDto reportRequestDto, List<DataCollectorLocation> dataCollectorLocations)
        {
            if (report.RawReport.Village == null && report.RawReport.Zone == null)
            {
                return true;
            }

            var location = dataCollectorLocations
                .SingleOrDefault(dcl => dcl.Village == report.RawReport.Village && (dcl.Zone == null || dcl.Zone == report.RawReport.Zone));

            return reportRequestDto.DataCollectorLocationId != location?.Id;
        }

        private async Task UpdateDataCollectorForReport(Report report, int dataCollectorId)
        {
            var newDataCollector = await _nyssContext.DataCollectors
                .Include(dc => dc.DataCollectorLocations).ThenInclude(dcl => dcl.Village)
                .Include(dc => dc.DataCollectorLocations).ThenInclude(dcl => dcl.Zone)
                .SingleOrDefaultAsync(dc => dc.Id == dataCollectorId);

            if (newDataCollector == null)
            {
                throw new ResultException(ResultKey.Report.Edit.SenderDoesNotExist);
            }

            if ((newDataCollector.DataCollectorType == DataCollectorType.CollectionPoint && report.ReportType != ReportType.DataCollectionPoint)
                || (newDataCollector.DataCollectorType == DataCollectorType.Human && report.ReportType == ReportType.DataCollectionPoint))
            {
                throw new ResultException(ResultKey.Report.Edit.DataCollectorTypeCannotBeChanged);
            }

            report.DataCollector = newDataCollector;
            report.RawReport.DataCollector = newDataCollector;
        }

        private async Task UpdateLocationForReport(Report report, int locationId, int dataCollectorId)
        {
            var location = await _nyssContext.DataCollectorLocations
                .Include(dcl => dcl.Village)
                .Include(dcl => dcl.Zone)
                .Where(dcl => dcl.DataCollectorId == dataCollectorId && dcl.Id == locationId)
                .SingleOrDefaultAsync();

            if (location == null)
            {
                throw new ResultException(ResultKey.Report.Edit.LocationNotFound);
            }

            report.Location = location.Location;
            report.RawReport.Village = location.Village;
            report.RawReport.Zone = location.Zone;
        }

        private void UpdateReportedCaseCountForReport(Report report, ReportRequestDto reportRequestDto)
        {
            if (report.ReportType != ReportType.Event)
            {
                report.ReportedCase.CountMalesBelowFive = reportRequestDto.CountMalesBelowFive;
                report.ReportedCase.CountMalesAtLeastFive = reportRequestDto.CountMalesAtLeastFive;
                report.ReportedCase.CountFemalesBelowFive = reportRequestDto.CountFemalesBelowFive;
                report.ReportedCase.CountFemalesAtLeastFive = reportRequestDto.CountFemalesAtLeastFive;
                report.ReportedCase.CountUnspecifiedSexAndAge = reportRequestDto.CountUnspecifiedSexAndAge;
            }

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

        internal static Area MapToArea(AreaDto area) =>
            area == null
                ? null
                : new Area
                {
                    AreaType = area.Type,
                    AreaId = area.Id
                };
    }
}
