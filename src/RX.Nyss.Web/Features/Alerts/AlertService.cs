using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Alerts
{
    public interface IAlertService
    {
        Task<Result<PaginatedList<AlertListItemResponseDto>>> List(int projectId, int pageNumber);
    }

    public class AlertService : IAlertService
    {
        private readonly INyssContext _nyssContext;
        private readonly IConfig _config;

        public AlertService(
            INyssContext nyssContext,
            IConfig config)
        {
            _nyssContext = nyssContext;
            _config = config;
        }

        public async Task<Result<PaginatedList<AlertListItemResponseDto>>> List(int projectId, int pageNumber)
        {
            var project = await _nyssContext.Projects.FindAsync(projectId);
            var projectTimeZone = TimeZoneInfo.FindSystemTimeZoneById(project.TimeZone);

            var alertsQuery = _nyssContext.Alerts
                .Where(a => a.ProjectHealthRisk.Project.Id == projectId);

            var rowsPerPage = _config.PaginationRowsPerPage;
            var totalCount = await alertsQuery.CountAsync();

            var alerts = await alertsQuery
                .Select(a => new
                {
                    a.CreatedAt,
                    a.Status,
                    ReportCount = a.AlertReports.Count,
                    LastReportVillage = a.AlertReports.OrderByDescending(r => r.Report.Id).First().Report.Village.Name,
                    HealthRisk = a.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == a.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name)
                        .Single()
                })
                .Page(pageNumber, rowsPerPage)
                .ToListAsync();

            var dtos = alerts
                .Select(a => new AlertListItemResponseDto
                {
                    CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(a.CreatedAt, projectTimeZone),
                    Status = a.Status.ToString(),
                    ReportCount = a.ReportCount,
                    LastReportVillage = a.LastReportVillage,
                    HealthRisk = a.HealthRisk
                })
                .AsPaginatedList(pageNumber, totalCount, rowsPerPage);

            return Success(dtos);
        }
    }
}
