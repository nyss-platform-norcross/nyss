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

namespace RX.Nyss.Web.Features.Report
{
    public interface IReportService
    {
        Task<PagedResult<List<ReportListResponseDto>>> List(int projectId, int pageNumber, string identityName, ListFilterRequestDto userIdentityName);
    }

    public class ReportService : IReportService
    {
        private readonly IConfig _config;
        private readonly INyssContext _nyssContext;
        private readonly IUserService _userService;

        public ReportService(INyssContext nyssContext, IUserService userService, IConfig config)
        {
            _nyssContext = nyssContext;
            _userService = userService;
            _config = config;
        }

        public async Task<PagedResult<List<ReportListResponseDto>>> List(int projectId, int pageNumber, string userIdentityName, ListFilterRequestDto filter)
        {
            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(userIdentityName);
            var rowsPerPage = _config.PaginationRowsPerPage;
            var baseQuery = _nyssContext.RawReports
                .Where(r => r.DataCollector.Project.Id == projectId)
                .Where(r => filter.ReportListType == ReportListTypeDto.Training ?
                      r.IsTraining.HasValue && r.IsTraining.Value :
                      r.IsTraining.HasValue && !r.IsTraining.Value);

            var rowCount = baseQuery.Count();

            var result = await baseQuery.Select(r => new ReportListResponseDto
                {
                    Id = r.Id,
                    DateTime = r.ReceivedAt,
                    HealthRiskName = r.Report.ProjectHealthRisk.HealthRisk.LanguageContents.Single(lc => lc.ContentLanguage.LanguageCode == userApplicationLanguageCode).Name,
                    IsValid = r.Report != null,
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

            var project = await _nyssContext.Projects.FindAsync(projectId);
            var projectTimeZone = TimeZoneInfo.FindSystemTimeZoneById(project.TimeZone);
            result.ForEach(x => x.DateTime = TimeZoneInfo.ConvertTimeFromUtc(x.DateTime, projectTimeZone));

            return PagedResult.Success(result, pageNumber, rowCount, rowsPerPage);
        }
    }
}
