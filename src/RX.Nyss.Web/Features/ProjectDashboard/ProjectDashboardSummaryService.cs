using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.ProjectDashboard.Dto;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Services.ReportsDashboard;

namespace RX.Nyss.Web.Features.ProjectDashboard
{
    public interface IProjectDashboardSummaryService
    {
        Task<ProjectSummaryResponseDto> GetData(ReportsFilter filters);
    }

    public class ProjectDashboardSummaryService : IProjectDashboardSummaryService
    {
        private readonly IReportService _reportService;

        private readonly INyssContext _nyssContext;

        private readonly IReportsDashboardSummaryService _reportsDashboardSummaryService;

        public ProjectDashboardSummaryService(
            IReportService reportService,
            INyssContext nyssContext,
            IReportsDashboardSummaryService reportsDashboardSummaryService)
        {
            _reportService = reportService;
            _nyssContext = nyssContext;
            _reportsDashboardSummaryService = reportsDashboardSummaryService;
        }

        public async Task<ProjectSummaryResponseDto> GetData(ReportsFilter filters)
        {
            if (!filters.ProjectId.HasValue)
            {
                throw new InvalidOperationException("ProjectId was not supplied");
            }

            var projectId = filters.ProjectId.Value;

            // Check if project exists
            var projectExists = await _nyssContext.Projects
                .AsNoTracking()
                .AnyAsync(p => p.Id == projectId);

            if (!projectExists)
            {
                throw new InvalidOperationException("Project not found");
            }

            // Keep as IQueryable for methods that need it
            var dashboardReportsQuery = _reportService.GetDashboardHealthRiskEventReportsQuery(filters);
            var rawReportsQuery = _reportService.GetRawReportsWithDataCollectorAndActivityReportsQuery(filters);

            // Materialize only required fields to avoid navigation-loading issues
            var dashboardReports = await dashboardReportsQuery
                .Select(r => new
                {
                    r.ReportedCaseCount
                })
                .ToListAsync();

            var rawReports = await rawReportsQuery
                .Select(r => new
                {
                    DataCollectorId = r.DataCollector != null ? r.DataCollector.Id : (int?)null,
                    DistrictId = r.Village != null && r.Village.District != null ? r.Village.District.Id : (int?)null,
                    VillageId = r.Village != null ? r.Village.Id : (int?)null
                })
                .ToListAsync();

            return new ProjectSummaryResponseDto
            {
                TotalReportCount = dashboardReports.Sum(r => r.ReportedCaseCount),
                ActiveDataCollectorCount = rawReports
                    .Select(r => r.DataCollectorId)
                    .OfType<int>()
                    .Distinct()
                    .Count(),
                DataCollectionPointSummary = _reportsDashboardSummaryService.DataCollectionPointsSummary(dashboardReportsQuery),
                AlertsSummary = _reportsDashboardSummaryService.AlertsSummary(filters),
                NumberOfDistricts = rawReports
                    .Select(r => r.DistrictId)
                    .OfType<int>()
                    .Distinct()
                    .Count(),
                NumberOfVillages = rawReports
                    .Select(r => r.VillageId)
                    .OfType<int>()
                    .Distinct()
                    .Count()
            };
        }
    }
}
