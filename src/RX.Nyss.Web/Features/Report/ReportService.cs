using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Project;
using RX.Nyss.Web.Features.Report.Dto;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Services.StringsResources;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Report
{
    public interface IReportService
    {
        Task<Result<PaginatedList<ReportListResponseDto>>> List(int projectId, int pageNumber, ReportListFilterRequestDto filter);
        Task<Result<ReportListFilterResponseDto>> GetReportFilters(int nationalSocietyId);
        Task<byte[]> Export(int projectId, ReportListFilterRequestDto filter);
        Task<Result> MarkAsError(int reportId);
    }

    public class ReportService : IReportService
    {
        private readonly IConfig _config;
        private readonly INyssContext _nyssContext;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IExcelExportService _excelExportService;
        private readonly IStringsResourcesService _stringsResourcesService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ReportService(INyssContext nyssContext, IUserService userService, IProjectService projectService, IConfig config, IAuthorizationService authorizationService, IExcelExportService excelExportService, IStringsResourcesService stringsResourcesService, IDateTimeProvider dateTimeProvider)
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

        public async Task<Result<PaginatedList<ReportListResponseDto>>> List(int projectId, int pageNumber, ReportListFilterRequestDto filter)
        {
            var (baseQuery, reportsQuery) = await GetReportQueries(projectId, filter);

            var rowsPerPage = _config.PaginationRowsPerPage;
            var reports = await reportsQuery
                .Page(pageNumber, rowsPerPage)
                .ToListAsync();

            await UpdateTimeZoneInReports(projectId, reports);

            return Success(reports.Cast<ReportListResponseDto>().AsPaginatedList(pageNumber, await baseQuery.CountAsync(), rowsPerPage));
        }

        private async Task UpdateTimeZoneInReports(int projectId, List<IReportListResponseDto> reports)
        {
            var project = await _nyssContext.Projects.FindAsync(projectId);
            var projectTimeZone = TimeZoneInfo.FindSystemTimeZoneById(project.TimeZone);
            reports.ForEach(x => x.DateTime = TimeZoneInfo.ConvertTimeFromUtc(x.DateTime, projectTimeZone));
        }

        private  async Task <(IQueryable<RawReport> baseQuery, IQueryable<IReportListResponseDto> result)> GetReportQueries(int projectId, ReportListFilterRequestDto filter)
        {
            var currentUser = _authorizationService.GetCurrentUser();
            var isSupervisor = _authorizationService.IsCurrentUserInRole(Role.Supervisor);
            var currentUserId = await _nyssContext.Users
                .Where(u => u.EmailAddress == currentUser.Name)
                .Select(u => u.Id)
                .SingleAsync();
            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(currentUser.Name);

            var baseQuery = _nyssContext.RawReports
                .Where(r => r.DataCollector.Project.Id == projectId)
                .Where(r => filter.ReportsType== ReportListType.FromDcp ?
                            r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint :
                            r.DataCollector.DataCollectorType == DataCollectorType.Human)
                .Where(r => filter.HealthRiskId == null || r.Report.ProjectHealthRisk.HealthRiskId == filter.HealthRiskId)
                .Where(r => filter.Status ? r.Report != null : r.Report == null)
                .Where(r => filter.IsTraining ?
                    r.IsTraining.HasValue && r.IsTraining.Value :
                    r.IsTraining.HasValue && !r.IsTraining.Value);

            baseQuery = FilterReportsByArea(baseQuery, filter.Area);

            var result = baseQuery.Select(r => new ReportListResponseDto
                {
                    Id = r.Id,
                    DateTime = r.ReceivedAt,
                    HealthRiskName = r.Report.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.LanguageCode == userApplicationLanguageCode)
                        .Select(lc => lc.Name)
                        .Single(),
                    IsValid = r.Report != null,
                    Region = r.Report != null
                        ? r.Report.Village.District.Region.Name
                        : r.DataCollector.Village.District.Region.Name,
                    District = r.Report != null
                        ? r.Report.Village.District.Name
                        : r.DataCollector.Village.District.Name,
                    Village = r.Report != null
                        ? r.Report.Village.Name
                        : r.DataCollector.Village.Name,
                    Zone = r.Report != null
                        ? r.Report.Zone != null
                            ? r.Report.Zone.Name
                            : null
                        : r.DataCollector.Zone != null
                            ? r.DataCollector.Zone.Name
                            : null,
                    DataCollectorDisplayName = r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint ? r.DataCollector.Name : r.DataCollector.DisplayName,
                    PhoneNumber = r.Sender,
                    IsMarkedAsError = r.Report.MarkedAsError,
                    UserHasAccessToReportDataCollector = !isSupervisor || r.DataCollector.Supervisor.Id == currentUserId,
                    IsInAlert = r.Report.ReportAlerts.Any(),
                    ReportId = r.ReportId,
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

            return (baseQuery, result);
        }

        private  async Task <(IQueryable<RawReport> baseQuery, IQueryable<IReportListResponseDto> result)> GetExportQueries(int projectId, ReportListFilterRequestDto filter)
        {
            var currentUser = _authorizationService.GetCurrentUser();
            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(currentUser.Name);

            var baseQuery = _nyssContext.RawReports
                .Where(r => r.DataCollector.Project.Id == projectId)
                .Where(r => filter.IsTraining ?
                      r.IsTraining.HasValue && r.IsTraining.Value :
                      r.IsTraining.HasValue && !r.IsTraining.Value)
                .Where(r => filter.ReportsType== ReportListType.FromDcp ?
                            r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint :
                            r.DataCollector.DataCollectorType == DataCollectorType.Human);

            var result = baseQuery.Select(r => new ExportReportListResponseDto
                {
                    Id = r.Id,
                    DateTime = r.ReceivedAt,
                    HealthRiskName = r.Report.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.LanguageCode == userApplicationLanguageCode)
                        .Select(lc => lc.Name)
                        .Single(),
                    IsValid = r.Report != null,
                    Region = r.Report != null
                        ? r.Report.Village.District.Region.Name
                        : r.DataCollector.Village.District.Region.Name,
                    District = r.Report != null
                        ? r.Report.Village.District.Name
                        : r.DataCollector.Village.District.Name,
                    Village = r.Report != null
                        ? r.Report.Village.Name
                        : r.DataCollector.Village.Name,
                    Zone = r.Report != null
                        ? r.Report.Zone != null
                            ? r.Report.Zone.Name
                            : null
                        : r.DataCollector.Zone != null
                            ? r.DataCollector.Zone.Name
                            : null,
                    Location = r.Report != null ? r.Report.Location : null,
                    DataCollectorDisplayName = r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint ? r.DataCollector.Name : r.DataCollector.DisplayName,
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

            return (baseQuery, result);
        }

        public async Task<Result<ReportListFilterResponseDto>> GetReportFilters(int projectId)
        {
            var projectHealthRiskNames = await _projectService.GetProjectHealthRiskNames(projectId);

            var dto = new ReportListFilterResponseDto
            {
                HealthRisks = projectHealthRiskNames
                    .Select(p => new HealthRiskDto { Id = p.Id, Name = p.Name })
            };

            return Success(dto);
        }

        //ToDo: use common logic with the project dashboard
        private static IQueryable<RawReport> FilterReportsByArea(IQueryable<RawReport> rawReports, AreaDto area) =>
            area?.Type switch
            {
                AreaDto.AreaType.Region =>
                rawReports.Where(r => r.Report != null ? r.Report.Village.District.Region.Id == area.Id : r.DataCollector.Village.District.Region.Id == area.Id),

                AreaDto.AreaType.District =>
                rawReports.Where(r => r.Report != null ? r.Report.Village.District.Id == area.Id : r.DataCollector.Village.District.Id == area.Id),

                AreaDto.AreaType.Village =>
                rawReports.Where(r => r.Report != null ? r.Report.Village.Id == area.Id : r.DataCollector.Village.Id == area.Id),

                AreaDto.AreaType.Zone =>
                rawReports.Where(r => r.Report != null ? r.Report.Zone.Id == area.Id : r.DataCollector.Zone.Id == area.Id),

                _ =>
                rawReports
            };

        public async Task<byte[]> Export(int projectId, ReportListFilterRequestDto filter)
        {
            var (baseQuery, reportsQuery) = await GetExportQueries(projectId, filter);

            var reports = await reportsQuery.ToListAsync();
            await UpdateTimeZoneInReports(projectId, reports);

            var excelSheet = await GetExcelData(reports);
            return excelSheet;
        }

        public async Task<byte[]> GetExcelData(List<IReportListResponseDto> reports)
        {
            var user = _authorizationService.GetCurrentUser();
            var userApplicationLanguage = _nyssContext.Users
                .Where(u => u.EmailAddress == user.Name)
                .Select(u => u.ApplicationLanguage.LanguageCode)
                .Single();

            var stringResources = (await _stringsResourcesService.GetStringsResources(userApplicationLanguage)).Value;

            var columnLabels = new List<string>()
            {
                GetStringResource(stringResources,"reports.export.date"),
                GetStringResource(stringResources,"reports.export.time"),
                GetStringResource(stringResources,"reports.list.status"),
                GetStringResource(stringResources,"reports.list.dataCollectorDisplayName"),
                GetStringResource(stringResources,"reports.list.dataCollectorPhoneNumber"),
                GetStringResource(stringResources,"reports.list.region"),
                GetStringResource(stringResources,"reports.list.district"),
                GetStringResource(stringResources,"reports.list.village"),
                GetStringResource(stringResources,"reports.list.zone"),
                GetStringResource(stringResources,"reports.list.healthRisk"),
                GetStringResource(stringResources,"reports.list.malesBelowFive"),
                GetStringResource(stringResources,"reports.list.malesAtLeastFive"),
                GetStringResource(stringResources,"reports.list.femalesBelowFive"),
                GetStringResource(stringResources,"reports.list.femalesAtLeastFive"),
                GetStringResource(stringResources,"reports.export.totalBelowFive"),
                GetStringResource(stringResources,"reports.export.totalAtLeastFive"),
                GetStringResource(stringResources,"reports.export.totalMale"),
                GetStringResource(stringResources,"reports.export.totalFemale"),
                GetStringResource(stringResources,"reports.export.total"),
                GetStringResource(stringResources,"reports.export.location"),
                GetStringResource(stringResources,"reports.export.message"),
                GetStringResource(stringResources,"reports.export.epiYear"),
                GetStringResource(stringResources,"reports.export.epiWeek")
            };

            var reportData = reports.Select(r => {
                var report = (ExportReportListResponseDto)r;
                return new
                {
                    Date = report.DateTime.ToString("yyyy-MM-dd"),
                    Time = report.DateTime.ToString("hh:mm"),
                    Status = report.IsValid
                        ? GetStringResource(stringResources, "reports.list.success")
                        : GetStringResource(stringResources, "reports.list.error"),
                    report.DataCollectorDisplayName,
                    report.PhoneNumber,
                    report.Region,
                    report.District,
                    report.Village,
                    report.Zone,
                    report.HealthRiskName,
                    report.CountMalesBelowFive,
                    report.CountMalesAtLeastFive,
                    report.CountFemalesBelowFive,
                    report.CountFemalesAtLeastFive,
                    TotalBelowFive = report.CountFemalesBelowFive + report.CountMalesBelowFive,
                    TotalAtLeastFive = report.CountMalesAtLeastFive + report.CountFemalesAtLeastFive,
                    TotalMale = report.CountMalesAtLeastFive + report.CountMalesBelowFive,
                    TotalFemale = report.CountFemalesAtLeastFive + report.CountFemalesBelowFive,
                    Total = report.CountMalesBelowFive + report.CountMalesAtLeastFive + report.CountFemalesBelowFive + report.CountFemalesAtLeastFive,
                    Location = report.Location != null ? $"{report.Location.Y}/{report.Location.Coordinate.X}" : "",
                    report.Message,
                    EpiYear = report.EpiWeek,
                    EpiWeek = report.EpiYear
                };
            });

            return _excelExportService.ToCsv(reportData, columnLabels);
        }

        private string GetStringResource(IDictionary<string, string> stringResources, string key) =>
            stringResources.Keys.Contains(key) ? stringResources[key] : key;

        public async Task<Result> MarkAsError(int reportId)
        {
            await SetMarkedAsError(reportId, true);
            return Success();
        }

        private async Task SetMarkedAsError(int reportId, bool markedAsError)
        {
            var report = await _nyssContext.Reports
                .Where(r => !r.ReportAlerts.Any())
                .FirstOrDefaultAsync(r => r.Id == reportId);

            report.MarkedAsError = markedAsError;
            await _nyssContext.SaveChangesAsync();
        }
    }
}
