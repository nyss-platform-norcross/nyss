using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Report.Dto;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Services.StringsResources;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Web.Utils.DataContract.Result;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Report
{
    public interface IReportService
    {
        Task<byte[]> Export(int projectId, ReportListFilterRequestDto filter);
        Task<Result<PaginatedList<ReportListResponseDto>>> List(int projectId, int pageNumber, ReportListFilterRequestDto filter);
    }

    public class ReportService : IReportService
    {
        private readonly IConfig _config;
        private readonly INyssContext _nyssContext;
        private readonly IUserService _userService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IExcelExportService _excelExportService;
        private readonly IStringsResourcesService _stringsResourcesService;
        private readonly IDateTimeProvider _dateTimeProvider;


        public ReportService(INyssContext nyssContext, IUserService userService, IConfig config, IAuthorizationService authorizationService, IExcelExportService excelExportService, IStringsResourcesService stringsResourcesService, IDateTimeProvider dateTimeProvider)
        {
            _nyssContext = nyssContext;
            _userService = userService;
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

            return Success(reports.AsPaginatedList(pageNumber, await baseQuery.CountAsync(), rowsPerPage));
        }

        private async Task UpdateTimeZoneInReports(int projectId, List<ReportListResponseDto> reports)
        {
            var project = await _nyssContext.Projects.FindAsync(projectId);
            var projectTimeZone = TimeZoneInfo.FindSystemTimeZoneById(project.TimeZone);
            reports.ForEach(x => x.DateTime = TimeZoneInfo.ConvertTimeFromUtc(x.DateTime, projectTimeZone));
        }

        private async Task UpdateTimeZoneInExportReports(int projectId, List<ExportListResponseDto> reports)
        {
            var project = await _nyssContext.Projects.FindAsync(projectId);
            var projectTimeZone = TimeZoneInfo.FindSystemTimeZoneById(project.TimeZone);
            reports.ForEach(x => x.DateTime = TimeZoneInfo.ConvertTimeFromUtc(x.DateTime, projectTimeZone));
        }

        private  async Task <(IQueryable<RawReport> baseQuery, IQueryable<ReportListResponseDto> result)> GetReportQueries(int projectId, ReportListFilterRequestDto filter)
        {
            var currentUser = _authorizationService.GetCurrentUser();
            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(currentUser.Name);

            var baseQuery = _nyssContext.RawReports
                .Where(r => r.DataCollector.Project.Id == projectId)
                .Where(r => filter.IsTraining ?
                      r.IsTraining.HasValue && r.IsTraining.Value :
                      r.IsTraining.HasValue && !r.IsTraining.Value)
                .Where(r => filter.ReportListType== ReportListType.FromDcp ?
                            r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint :
                            r.DataCollector.DataCollectorType == DataCollectorType.Human);

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
                    CountMalesBelowFive = r.Report.ReportedCase.CountMalesBelowFive,
                    CountMalesAtLeastFive = r.Report.ReportedCase.CountMalesAtLeastFive,
                    CountFemalesBelowFive = r.Report.ReportedCase.CountFemalesBelowFive,
                    CountFemalesAtLeastFive = r.Report.ReportedCase.CountFemalesAtLeastFive,
                    ReferredCount = r.Report.DataCollectionPointCase.ReferredCount,
                    DeathCount = r.Report.DataCollectionPointCase.DeathCount,
                    FromOtherVillagesCount = r.Report.DataCollectionPointCase.FromOtherVillagesCount
                })
                .OrderByDescending(r => r.DateTime);

            return (baseQuery, result);
        }

        private  async Task <(IQueryable<RawReport> baseQuery, IQueryable<ExportListResponseDto> result)> GetExportQueries(int projectId, ReportListFilterRequestDto filter)
        {
            var currentUser = _authorizationService.GetCurrentUser();
            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(currentUser.Name);

            var baseQuery = _nyssContext.RawReports
                .Where(r => r.DataCollector.Project.Id == projectId)
                .Include(r => r.DataCollector)
                .Where(r => filter.IsTraining ?
                      r.IsTraining.HasValue && r.IsTraining.Value :
                      r.IsTraining.HasValue && !r.IsTraining.Value)
                .Where(r => filter.ReportListType== ReportListType.FromDcp ?
                            r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint :
                            r.DataCollector.DataCollectorType == DataCollectorType.Human);

            var result = baseQuery.Select(r => new ExportListResponseDto
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
                    CountMalesBelowFive = r.Report.ReportedCase.CountMalesBelowFive,
                    CountMalesAtLeastFive = r.Report.ReportedCase.CountMalesAtLeastFive,
                    CountFemalesBelowFive = r.Report.ReportedCase.CountFemalesBelowFive,
                    CountFemalesAtLeastFive = r.Report.ReportedCase.CountFemalesAtLeastFive,
                    ReferredCount = r.Report.DataCollectionPointCase.ReferredCount,
                    DeathCount = r.Report.DataCollectionPointCase.DeathCount,
                    FromOtherVillagesCount = r.Report.DataCollectionPointCase.FromOtherVillagesCount
                })
                .OrderByDescending(r => r.DateTime);

            return (baseQuery, result);
        }

        public async Task<byte[]> Export(int projectId, ReportListFilterRequestDto filter)
        {
            var (baseQuery, reportsQuery) = await GetExportQueries(projectId, filter);

            var reports = await reportsQuery.ToListAsync();
            await UpdateTimeZoneInExportReports(projectId, reports);

            var excelSheet = await GetExcelData(reports);
            return excelSheet;
        }

        public async Task<byte[]> GetExcelData(List<ExportListResponseDto> reports)
        {
            var user = _authorizationService.GetCurrentUser();
            var userApplicationLanguage = _nyssContext.Users
                .Where(u => u.EmailAddress == user.Name)
                .Select(u => u.ApplicationLanguage.LanguageCode)
                .Single();

            var stringResources = (await _stringsResourcesService.GetStringsResources(userApplicationLanguage)).Value;

            var columnLabels = new List<string>()
            {
                GetStringResource(stringResources,"reports.list.time"),
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
                GetStringResource(stringResources,"reports.export.totalFemale"),
                GetStringResource(stringResources,"reports.export.totalMale"),
                GetStringResource(stringResources,"reports.export.total"),
                GetStringResource(stringResources,"reports.export.location"),
                GetStringResource(stringResources,"reports.export.message"),
                GetStringResource(stringResources,"reports.export.epiYear"),
                GetStringResource(stringResources,"reports.export.epiWeek")
            };

            var reportData = reports.Select(r => new
            {
                Date = r.DateTime.ToString("yyyy-MM-dd"),
                Time = r.DateTime.ToString("hh:mm"),
                Status = r.IsValid
                    ? GetStringResource(stringResources, "reports.list.success")
                    : GetStringResource(stringResources, "reports.list.error"),
                r.DataCollectorDisplayName,
                r.PhoneNumber,
                r.Region,
                r.District,
                r.Village,
                r.Zone,
                r.HealthRiskName,
                r.CountMalesBelowFive,
                r.CountMalesAtLeastFive,
                r.CountFemalesBelowFive,
                r.CountFemalesAtLeastFive,
                TotalBelowFive = r.CountFemalesBelowFive + r.CountMalesBelowFive,
                TotalAtLeastFive = r.CountMalesAtLeastFive + r.CountFemalesAtLeastFive,
                TotalFemale = r.CountFemalesAtLeastFive + r.CountFemalesBelowFive,
                TotalMale = r.CountMalesAtLeastFive + r.CountMalesBelowFive,
                Total = r.CountMalesBelowFive + r.CountMalesAtLeastFive + r.CountFemalesBelowFive + r.CountFemalesAtLeastFive,
                Location = r.DataCollector != null ? $"{r.DataCollector.Location.X}/{r.DataCollector.Location.Y}" : "",
                EpiYear = r.DateTime.Year,
                EpiWeek = _dateTimeProvider.GetEpiWeek(r.DateTime)
            });

            return _excelExportService.ToCsv(reportData, columnLabels);
        }

        private string GetStringResource(IDictionary<string, string> stringResources, string key) =>
            stringResources.Keys.Contains(key) ? stringResources[key] : key;
    }
}
