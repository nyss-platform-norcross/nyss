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

namespace RX.Nyss.Web.Features.Report
{
    public interface IReportService
    {
        Task<Result<PaginatedList<ReportListResponseDto>>> List(int projectId, int pageNumber,  ListFilterRequestDto filter);
        Task<byte[]> Export(int projectId, ReportListType reportListType);
    }

    public class ReportService : IReportService
    {
        private readonly IConfig _config;
        private readonly INyssContext _nyssContext;
        private readonly IUserService _userService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IExcelExportService _excelExportService;
        private readonly IStringsResourcesService _stringsResourcesService;
        

        public ReportService(INyssContext nyssContext, IUserService userService, IConfig config, IAuthorizationService authorizationService, IExcelExportService excelExportService, IStringsResourcesService stringsResourcesService)
        {
            _nyssContext = nyssContext;
            _userService = userService;
            _config = config;
            _authorizationService = authorizationService;
            _excelExportService = excelExportService;
            _stringsResourcesService = stringsResourcesService;
        }

        public async Task<Result<PaginatedList<ReportListResponseDto>>> List(int projectId, int pageNumber, ListFilterRequestDto filter)
        {
            var (baseQuery, reportsQuery) = await GetReportQueries(projectId, filter.ReportListType);

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

        private  async Task <(IQueryable<RawReport> baseQuery, IQueryable<ReportListResponseDto> result)> GetReportQueries(int projectId, ReportListType reportListType)
        {
            var currentUser = _authorizationService.GetCurrentUser();
            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(currentUser.Name);

            var baseQuery = _nyssContext.RawReports
                .Where(r => r.DataCollector.Project.Id == projectId)
                .Where(r => filter.IsTraining ?
                      r.IsTraining.HasValue && r.IsTraining.Value :
                      r.IsTraining.HasValue && !r.IsTraining.Value)
                .Where(r => filter.ReportListType == ReportListType.FromDcp ?
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
                    Region = r.Report.Village.District.Region.Name,
                    District = r.Report.Village.District.Name,
                    Village = r.Report.Village.Name,
                    Zone = r.Report.Zone.Name,
                    DataCollectorDisplayName = r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint ? r.DataCollector.Name : r.DataCollector.DisplayName,
                    PhoneNumber = r.Sender,
                    CountMalesBelowFive = r.Report.ReportedCase.CountMalesBelowFive,
                    CountFemalesBelowFive = r.Report.ReportedCase.CountFemalesBelowFive,
                    CountMalesAtLeastFive = r.Report.ReportedCase.CountMalesAtLeastFive,
                    CountFemalesAtLeastFive = r.Report.ReportedCase.CountFemalesAtLeastFive,
                    ReferredCount = r.Report.DataCollectionPointCase.ReferredCount,
                    DeathCount = r.Report.DataCollectionPointCase.DeathCount,
                    FromOtherVillagesCount = r.Report.DataCollectionPointCase.FromOtherVillagesCount
                })
                .OrderByDescending(r => r.DateTime);

            return (baseQuery, result);
        }

        public async Task<byte[]> Export(int projectId, ReportListType reportListType)
        {
            var (baseQuery, reportsQuery) = await GetReportQueries(projectId, reportListType);

            var reports = await reportsQuery.ToListAsync();
            await UpdateTimeZoneInReports(projectId, reports);

            var excelSheet = await GetExcelData(reports);
            return excelSheet;
        }

        public async Task<byte[]> GetExcelData(List<ReportListResponseDto> reports)
        {
            var user = _authorizationService.GetCurrentUser();
            var userApplicationLanguage = _nyssContext.Users
                .Where(u => u.EmailAddress == user.Name)
                .Select(u => u.ApplicationLanguage.LanguageCode)
                .Single();

            var stringResources = (await _stringsResourcesService.GetStringsResources(userApplicationLanguage)).Value;

            var columnLabels = new List<string>()
            {
                GetStringResource(stringResources,"reports.list.date"),
                GetStringResource(stringResources,"reports.list.time"),
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
                GetStringResource(stringResources,"reports.list.femalesAtLeastFive")
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
                r.CountFemalesBelowFive,
                r.CountMalesAtLeastFive,
                r.CountFemalesAtLeastFive
            });

            return _excelExportService.ToCsv(reportData, columnLabels);
        }

        private string GetStringResource(IDictionary<string, string> stringResources, string key) =>
            stringResources.Keys.Contains(key) ? stringResources[key] : key;
    }
}
