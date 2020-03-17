using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.ProjectDashboard.Dto;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Services.GeographicalCoverage;
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
        private readonly IGeographicalCoverageService _geographicalCoverageService;

        public ProjectDashboardSummaryService(
            IReportService reportService,
            INyssContext nyssContext,
            IReportsDashboardSummaryService reportsDashboardSummaryService,
            IGeographicalCoverageService geographicalCoverageService)
        {
            _reportService = reportService;
            _nyssContext = nyssContext;
            _reportsDashboardSummaryService = reportsDashboardSummaryService;
            _geographicalCoverageService = geographicalCoverageService;
        }

        public async Task<ProjectSummaryResponseDto> GetData(ReportsFilter filters)
        {
            if (!filters.ProjectId.HasValue)
            {
                throw new InvalidOperationException("ProjectId was not supplied");
            }

            var healthRiskEventReportsQuery = _reportService.GetHealthRiskEventReportsQuery(filters);
            var validReports = _reportService.GetSuccessReportsQuery(filters);
            var rawReportsWithDataCollector = _reportService.GetRawReportsWithDataCollectorQuery(filters);

            return await _nyssContext.Projects
                .Where(ph => ph.Id == filters.ProjectId.Value)
                .Select(ph => new
                {
                    allDataCollectorCount = AllDataCollectorCount(filters),
                    activeDataCollectorCount = rawReportsWithDataCollector.Select(r => r.DataCollector.Id).Distinct().Count()
                })
                .Select(data => new ProjectSummaryResponseDto
                {
                    ReportCount = healthRiskEventReportsQuery.Sum(r => r.ReportedCaseCount),
                    ActiveDataCollectorCount = data.activeDataCollectorCount,
                    InactiveDataCollectorCount = data.allDataCollectorCount - data.activeDataCollectorCount,
                    ErrorReportCount = rawReportsWithDataCollector.Count() - validReports.Count(),
                    DataCollectionPointSummary = _reportsDashboardSummaryService.DataCollectionPointsSummary(healthRiskEventReportsQuery),
                    AlertsSummary = _reportsDashboardSummaryService.AlertsSummary(filters),
                    NumberOfDistricts = _geographicalCoverageService.GetNumberOfDistrictsByProject(filters.ProjectId.Value, filters.StartDate).Count(),
                    NumberOfVillages = _geographicalCoverageService.GetNumberOfVillagesByProject(filters.ProjectId.Value, filters.StartDate).Count()
                })
                .FirstOrDefaultAsync();
        }

        private int AllDataCollectorCount(ReportsFilter filters) =>
            _nyssContext.DataCollectors
                .FilterByArea(filters.Area)
                .FilterByType(filters.DataCollectorType)
                .FilterByProject(filters.ProjectId.Value)
                .FilterByTrainingMode(filters.IsTraining)
                .FilterOnlyNotDeletedBefore(filters.StartDate)
                .Count();
    }
}
