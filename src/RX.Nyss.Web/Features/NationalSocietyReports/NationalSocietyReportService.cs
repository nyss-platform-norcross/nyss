using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.NationalSociety;
using RX.Nyss.Web.Features.NationalSocietyReports.Dto;
using RX.Nyss.Web.Features.Project;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.NationalSocietyReports
{
    public interface INationalSocietyReportService
    {
        Task<Result<PaginatedList<NationalSocietyReportListResponseDto>>> List(int nationalSocietyId, int pageNumber, NationalSocietyReportListFilterRequestDto filter);
        Task<Result<NationalSocietyReportListFilterResponseDto>> GetNationalSocietyReportFilters(int nationalSocietyId);
    }

    public class NationalSocietyReportService : INationalSocietyReportService
    {
        private readonly INyssWebConfig _config;
        private readonly INyssContext _nyssContext;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;
        private readonly INationalSocietyService _nationalSocietyService;
        private readonly IAuthorizationService _authorizationService;

        public NationalSocietyReportService(INyssContext nyssContext, IUserService userService, IProjectService projectService, INationalSocietyService nationalSocietyService, INyssConfig config, IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _userService = userService;
            _projectService = projectService;
            _nationalSocietyService = nationalSocietyService;
            _config = config;
            _authorizationService = authorizationService;
        }

        public async Task<Result<PaginatedList<NationalSocietyReportListResponseDto>>> List(int nationalSocietyId, int pageNumber, NationalSocietyReportListFilterRequestDto filter)
        {
            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(_authorizationService.GetCurrentUserName());
            var supervisorProjectIds = await _projectService.GetSupervisorProjectIds(_authorizationService.GetCurrentUserName());
            var rowsPerPage = _config.PaginationRowsPerPage;

            var baseQuery = FilterReportsByArea(_nyssContext.RawReports, filter.Area)
                .Where(r => r.NationalSociety.Id == nationalSocietyId)
                .Where(r => r.IsTraining == null || r.IsTraining == false)
                .Where(r =>
                    filter.ReportsType == NationalSocietyReportListType.FromDcp ? r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint :
                    filter.ReportsType == NationalSocietyReportListType.Main ? r.DataCollector.DataCollectorType == DataCollectorType.Human :
                    r.DataCollector == null)
                .Where(r => filter.HealthRiskId == null || r.Report.ProjectHealthRisk.HealthRiskId == filter.HealthRiskId)
                .Where(r => filter.Status ? r.Report != null && !r.Report.MarkedAsError : r.Report == null || (r.Report != null && r.Report.MarkedAsError));

            if (_authorizationService.IsCurrentUserInRole(Role.Supervisor))
            {
                baseQuery = baseQuery
                    .Where(r => r.DataCollector == null || supervisorProjectIds == null || supervisorProjectIds.Contains(r.DataCollector.Project.Id));
            }

            var result = await baseQuery.Select(r => new NationalSocietyReportListResponseDto
                {
                    Id = r.Id,
                    DateTime = r.ReceivedAt,
                    HealthRiskName = r.Report.ProjectHealthRisk.HealthRisk.LanguageContents.Where(lc => lc.ContentLanguage.LanguageCode == userApplicationLanguageCode).Select(lc => lc.Name).Single(),
                    IsValid = r.Report != null,
                    IsMarkedAsError = r.Report.MarkedAsError,
                    ProjectName = r.Report != null ? r.Report.ProjectHealthRisk.Project.Name : r.DataCollector.Project.Name,
                    ProjectTimeZone = r.Report != null ?  r.Report.ProjectHealthRisk.Project.TimeZone : r.DataCollector.Project.TimeZone,
                    Region = r.Report != null ? r.Report.Village.District.Region.Name : r.DataCollector.Village.District.Region.Name,
                    District = r.Report != null ? r.Report.Village.District.Name : r.DataCollector.Village.District.Name,
                    Village = r.Report != null ? r.Report.Village.Name : r.DataCollector.Village.Name,
                    Zone = r.Report != null ? r.Report.Zone.Name : r.DataCollector.Zone.Name,
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
                //ToDo: order base on filter.OrderBy property
                .OrderBy(r => r.DateTime, filter.SortAscending)
                .Page(pageNumber, rowsPerPage)
                .ToListAsync();

            foreach (var report in result.Where(r => r.ProjectTimeZone != null))
            {
                report.DateTime = TimeZoneInfo.ConvertTimeFromUtc(report.DateTime, TimeZoneInfo.FindSystemTimeZoneById(report.ProjectTimeZone));
            }
            
            return Success(result.AsPaginatedList(pageNumber, await baseQuery.CountAsync(), rowsPerPage));
        }

        public async Task<Result<NationalSocietyReportListFilterResponseDto>> GetNationalSocietyReportFilters(int nationalSocietyId)
        {
            var nationalSocietyHealthRiskNames = await _nationalSocietyService.GetNationalSocietyHealthRiskNames(nationalSocietyId);

            var dto = new NationalSocietyReportListFilterResponseDto
            {
                HealthRisks = nationalSocietyHealthRiskNames
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
    }
}
