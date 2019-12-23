using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.NationalSocietyReports.Dto;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.NationalSocietyReports
{
    public interface INationalSocietyReportService
    {
        Task<Result<PaginatedList<NationalSocietyReportListResponseDto>>> List(int nationalSocietyId, int pageNumber, string identityName);
    }

    public class NationalSocietyReportService : INationalSocietyReportService
    {
        private readonly IConfig _config;
        private readonly INyssContext _nyssContext;
        private readonly IUserService _userService;

        public NationalSocietyReportService(INyssContext nyssContext, IUserService userService, IConfig config)
        {
            _nyssContext = nyssContext;
            _userService = userService;
            _config = config;
        }

        public async Task<Result<PaginatedList<NationalSocietyReportListResponseDto>>> List(int nationalSocietyId, int pageNumber, string userIdentityName)
        {
            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(userIdentityName);
            var supervisorProjectIds = await _userService.GetSupervisorProjectIds(userIdentityName);
            var rowsPerPage = _config.PaginationRowsPerPage;

            var baseQuery = _nyssContext.RawReports
                .Where(r => r.NationalSociety.Id == nationalSocietyId)
                .Where(r => r.DataCollector == null || supervisorProjectIds == null || supervisorProjectIds.Contains(r.DataCollector.Project.Id));

            var result = await baseQuery.Select(r => new NationalSocietyReportListResponseDto
                {
                    Id = r.Id,
                    DateTime = r.ReceivedAt,
                    HealthRiskName = r.Report.ProjectHealthRisk.HealthRisk.LanguageContents.Where(lc => lc.ContentLanguage.LanguageCode == userApplicationLanguageCode).Select(lc => lc.Name).Single(),
                    IsValid = r.Report != null,
                    ProjectName = r.Report.ProjectHealthRisk.Project.Name,
                    ProjectTimeZone = r.Report.ProjectHealthRisk.Project.TimeZone,
                    Region = r.DataCollector.Village.District.Region.Name,
                    District = r.DataCollector.Village.District.Name,
                    Village = r.DataCollector.Village.Name,
                    Zone = r.DataCollector.Zone != null
                        ? r.DataCollector.Zone.Name
                        : null,
                    DataCollectorDisplayName = r.DataCollector.DisplayName,
                    PhoneNumber = r.DataCollector.PhoneNumber,
                    CountMalesBelowFive = r.Report.ReportedCase.CountMalesBelowFive,
                    CountFemalesBelowFive = r.Report.ReportedCase.CountFemalesBelowFive,
                    CountMalesAtLeastFive = r.Report.ReportedCase.CountMalesAtLeastFive,
                    CountFemalesAtLeastFive = r.Report.ReportedCase.CountFemalesAtLeastFive
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
