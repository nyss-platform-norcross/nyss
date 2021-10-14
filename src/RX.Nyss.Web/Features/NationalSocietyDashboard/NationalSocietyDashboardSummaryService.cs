using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.NationalSocietyDashboard.Dto;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Services.ReportsDashboard;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard
{
    public interface INationalSocietyDashboardSummaryService
    {
        Task<NationalSocietySummaryResponseDto> GetData(ReportsFilter filters);
    }

    public class NationalSocietyDashboardSummaryService : INationalSocietyDashboardSummaryService
    {
        private readonly IReportService _reportService;

        private readonly INyssContext _nyssContext;

        private readonly IReportsDashboardSummaryService _reportsDashboardSummaryService;

        public NationalSocietyDashboardSummaryService(
            IReportService reportService,
            INyssContext nyssContext,
            IReportsDashboardSummaryService reportsDashboardSummaryService)
        {
            _reportService = reportService;
            _nyssContext = nyssContext;
            _reportsDashboardSummaryService = reportsDashboardSummaryService;
        }

        public async Task<NationalSocietySummaryResponseDto> GetData(ReportsFilter filters)
        {
            if (!filters.NationalSocietyId.HasValue)
            {
                throw new InvalidOperationException("NationalSocietyId was not supplied");
            }

            var nationalSocietyId = filters.NationalSocietyId.Value;

            var dashboardReports = _reportService.GetDashboardHealthRiskEventReportsQuery(filters);
            var rawReportsWithDataCollector = _reportService.GetRawReportsWithDataCollectorQuery(filters);

            return await _nyssContext.NationalSocieties
                .Where(ns => ns.Id == nationalSocietyId)
                .Select(ns => new
                {
                    allDataCollectorCount = AllDataCollectorCount(filters, nationalSocietyId),
                    activeDataCollectorCount = rawReportsWithDataCollector.Select(r => r.DataCollector.Id).Distinct().Count()
                })
                .Select(data => new NationalSocietySummaryResponseDto
                {
                    KeptReportCount = dashboardReports.Where(r => r.Status == ReportStatus.Accepted).Sum(r => r.ReportedCaseCount),
                    DismissedReportCount = dashboardReports.Where(r => r.Status == ReportStatus.Rejected).Sum(r => r.ReportedCaseCount),
                    NotCrossCheckedReportCount = dashboardReports.Where(r => r.Status == ReportStatus.New || r.Status == ReportStatus.Pending || r.Status == ReportStatus.Closed).Sum(r => r.ReportedCaseCount),
                    TotalReportCount = dashboardReports.Sum(r => r.ReportedCaseCount),
                    ActiveDataCollectorCount = data.activeDataCollectorCount,
                    InactiveDataCollectorCount = data.allDataCollectorCount - data.activeDataCollectorCount,
                    DataCollectionPointSummary = _reportsDashboardSummaryService.DataCollectionPointsSummary(dashboardReports),
                    AlertsSummary = _reportsDashboardSummaryService.AlertsSummary(filters),
                    NumberOfDistricts = rawReportsWithDataCollector.Select(r => r.Village.District).Distinct().Count(),
                    NumberOfVillages = rawReportsWithDataCollector.Select(r => r.Village).Distinct().Count()
                })
                .FirstOrDefaultAsync();
        }

        private int AllDataCollectorCount(ReportsFilter filters, int nationalSocietyId) =>
            _nyssContext.DataCollectors
                .FilterByArea(filters.Area)
                .FilterByType(filters.DataCollectorType)
                .FilterByNationalSociety(nationalSocietyId)
                .FilterByTrainingMode(filters.TrainingStatus)
                .FilterOnlyNotDeletedBefore(filters.StartDate)
                .Count();
    }
}
