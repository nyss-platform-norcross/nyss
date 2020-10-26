using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common.Extensions;
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

            var validReports = _reportService.GetSuccessReportsQuery(filters);
            var rawReportsWithDataCollector = _reportService.GetRawReportsWithDataCollectorQuery(filters);

            return await _nyssContext.Projects
                .Where(p => p.Id == filters.ProjectId.Value)
                .Select(p => new
                {
                    AllDataCollectorCount = AllDataCollectorCount(filters),
                    ActiveDataCollectorCount = rawReportsWithDataCollector.Select(r => r.DataCollector.Id).Distinct().Count()
                })
                .Select(data => new ProjectSummaryResponseDto
                {
                    ReportCount = validReports.Sum(r => r.ReportedCaseCount),
                    DismissedReportCount = validReports.Where(r => r.Status == ReportStatus.Rejected).Sum(r => r.ReportedCaseCount),
                    KeptReportCount = validReports.Where(r => r.Status == ReportStatus.Accepted).Sum(r => r.ReportedCaseCount),
                    PendingReportCount = validReports.Where(r => r.Status == ReportStatus.Pending).Sum(r => r.ReportedCaseCount),
                    ActiveDataCollectorCount = data.ActiveDataCollectorCount,
                    InactiveDataCollectorCount = data.AllDataCollectorCount - data.ActiveDataCollectorCount,
                    ErrorReportCount = rawReportsWithDataCollector.Count() - validReports.Count(),
                    DataCollectionPointSummary = _reportsDashboardSummaryService.DataCollectionPointsSummary(validReports),
                    AlertsSummary = _reportsDashboardSummaryService.AlertsSummary(filters),
                    NumberOfDistricts = rawReportsWithDataCollector.Select(r => r.Village.District).Distinct().Count(),
                    NumberOfVillages = rawReportsWithDataCollector.Select(r => r.Village).Distinct().Count()
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
