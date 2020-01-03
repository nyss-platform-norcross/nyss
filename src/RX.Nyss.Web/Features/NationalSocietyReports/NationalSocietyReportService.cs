using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.NationalSocietyReports.Dto;
using RX.Nyss.Web.Features.Project;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.NationalSocietyReports
{
    public interface INationalSocietyReportService
    {
        Task<Result<PaginatedList<NationalSocietyReportListResponseDto>>> List(int nationalSocietyId, int pageNumber, string identityName, NationalSocietyReportListFilterRequestDto filter);
    }

    public class NationalSocietyReportService : INationalSocietyReportService
    {
        private readonly IConfig _config;
        private readonly INyssContext _nyssContext;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;

        public NationalSocietyReportService(INyssContext nyssContext, IUserService userService, IProjectService projectService, IConfig config)
        {
            _nyssContext = nyssContext;
            _userService = userService;
            _projectService = projectService;
            _config = config;
        }

        public async Task<Result<PaginatedList<NationalSocietyReportListResponseDto>>> List(int nationalSocietyId, int pageNumber, string userIdentityName, NationalSocietyReportListFilterRequestDto filter)
        {
            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(userIdentityName);
            var supervisorProjectIds = await _projectService.GetSupervisorProjectIds(userIdentityName);
            var rowsPerPage = _config.PaginationRowsPerPage;

            var baseQuery = _nyssContext.RawReports
                .Where(r => r.NationalSociety.Id == nationalSocietyId)
                .Where(r => r.IsTraining == null || r.IsTraining == false)
                .Where(r => r.DataCollector == null || supervisorProjectIds == null || supervisorProjectIds.Contains(r.DataCollector.Project.Id))
                .Where(r =>
                    filter.ReportListType == NationalSocietyReportListType.FromDcp ? r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint :
                    filter.ReportListType == NationalSocietyReportListType.Main ? r.DataCollector.DataCollectorType == DataCollectorType.Human :
                    r.DataCollector == null);

            var result = await baseQuery.Select(r => new NationalSocietyReportListResponseDto
                {
                    Id = r.Id,
                    DateTime = r.ReceivedAt,
                    HealthRiskName = r.Report.ProjectHealthRisk.HealthRisk.LanguageContents.Where(lc => lc.ContentLanguage.LanguageCode == userApplicationLanguageCode).Select(lc => lc.Name).Single(),
                    IsValid = r.Report != null,
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
                .OrderByDescending(r => r.DateTime)
                .Page(pageNumber, rowsPerPage)
                .ToListAsync();

            foreach (var report in result.Where(r => r.ProjectTimeZone != null))
            {
                report.DateTime = TimeZoneInfo.ConvertTimeFromUtc(report.DateTime, TimeZoneInfo.FindSystemTimeZoneById(report.ProjectTimeZone));
            }
            
            return Success(result.AsPaginatedList(pageNumber, await baseQuery.CountAsync(), rowsPerPage));
        }
    }
}
