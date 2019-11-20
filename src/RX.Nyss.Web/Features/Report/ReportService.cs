using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Report.Dto;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Report
{
    public interface IReportService
    {
        Task<PagedResult<List<ReportListResponseDto>>> List(int projectId, int pageNumber, string userIdentityName);
    }

    public class ReportService: IReportService
    {
        private readonly INyssContext _nyssContext;
        private readonly IUserService _userService;
        private readonly IConfig _config;

        public ReportService(INyssContext nyssContext, IUserService userService, IConfig config)
        {
            _nyssContext = nyssContext;
            _userService = userService;
            _config = config;
        }

        public async Task<PagedResult<List<ReportListResponseDto>>> List(int projectId, int pageNumber, string userIdentityName)
        {
            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(userIdentityName);
            var rowsPerPage = _config.PaginationRowsPerPage;
            var baseQuery = _nyssContext.Reports
                .Where(r => r.DataCollector.Project.Id == projectId);

            var rowCount = baseQuery.Count();

            var result = await baseQuery.Select(r => new ReportListResponseDto
                {
                    Id = r.Id,
                    DateTime = r.CreatedAt,
                    HealthRiskName = r.ProjectHealthRisk.HealthRisk.LanguageContents.Single(lc => lc.ContentLanguage.LanguageCode == userApplicationLanguageCode).Name,
                    Status = r.Status.ToString(),
                    Region = r.DataCollector.Village.District.Region.Name,
                    District = r.DataCollector.Village.District.Name,
                    Village = r.DataCollector.Village.Name,
                    Zone = r.DataCollector.Zone != null
                        ? r.DataCollector.Zone.Name
                        : null,
                    DataCollectorDisplayName = r.DataCollector.DisplayName,
                    DataCollectorPhoneNumber = r.DataCollector.PhoneNumber,
                    CountMalesBelowFive = r.ReportedCase.CountMalesBelowFive,
                    CountFemalesBelowFive = r.ReportedCase.CountFemalesBelowFive,
                    CountMalesAtLeastFive = r.ReportedCase.CountMalesAtLeastFive,
                    CountFemalesAtLeastFive = r.ReportedCase.CountFemalesAtLeastFive,
                })
                .Page(pageNumber, rowsPerPage)
                .ToListAsync();

            var project = await _nyssContext.Projects.FindAsync(projectId);
            var projectTimeZone = TimeZoneInfo.FindSystemTimeZoneById(project.TimeZone);
            result.ForEach(x => x.DateTime = TimeZoneInfo.ConvertTimeFromUtc(x.DateTime, projectTimeZone ));

            return PagedResult.Success(result, pageNumber, rowCount, rowsPerPage);
        }
    }
}
