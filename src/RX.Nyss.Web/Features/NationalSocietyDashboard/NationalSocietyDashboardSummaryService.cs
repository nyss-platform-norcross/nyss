using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.NationalSocietyDashboard.Dto;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Services.GeographicalCoverage;
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
        private readonly IGeographicalCoverageService _geographicalCoverageService;

        public NationalSocietyDashboardSummaryService(
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

        public async Task<NationalSocietySummaryResponseDto> GetData(ReportsFilter filters)
        {
            if (!filters.NationalSocietyId.HasValue)
            {
                throw new InvalidOperationException("NationalSocietyId was not supplied");
            }

            var nationalSocietyId = filters.NationalSocietyId.Value;

            var rawReportsWithDataCollector = _reportService.GetRawReportsWithDataCollectorQuery(filters);
            var successReports = _reportService.GetSuccessReportsQuery(filters);
            var healthRiskEventReportsQuery = _reportService.GetHealthRiskEventReportsQuery(filters);

            return await _nyssContext.NationalSocieties
                .Where(ns => ns.Id == nationalSocietyId)
                .Select(ns => new
                {
                    allDataCollectorCount = AllDataCollectorCount(filters, nationalSocietyId),
                    activeDataCollectorCount = rawReportsWithDataCollector.Select(r => r.DataCollector.Id).Distinct().Count()
                })
                .Select(data => new NationalSocietySummaryResponseDto
                {
                    ReportCount = healthRiskEventReportsQuery.Sum(r => r.ReportedCaseCount),
                    ActiveDataCollectorCount = data.activeDataCollectorCount,
                    InactiveDataCollectorCount = data.allDataCollectorCount - data.activeDataCollectorCount,
                    ErrorReportCount = rawReportsWithDataCollector.Count() - successReports.Count(),
                    DataCollectionPointSummary = _reportsDashboardSummaryService.DataCollectionPointsSummary(healthRiskEventReportsQuery),
                    AlertsSummary = _reportsDashboardSummaryService.AlertsSummary(filters),
                    NumberOfDistricts = filters.ProjectId.HasValue ?
                        _geographicalCoverageService.GetNumberOfDistrictsByProject(filters.ProjectId.Value, filters).Count()
                        : _geographicalCoverageService.GetNumberOfDistrictsByNationalSociety(filters.NationalSocietyId.Value, filters).Count(),
                    NumberOfVillages = filters.ProjectId.HasValue ?
                        _geographicalCoverageService.GetNumberOfVillagesByProject(filters.ProjectId.Value, filters).Count()
                        : _geographicalCoverageService.GetNumberOfVillagesByNationalSociety(filters.NationalSocietyId.Value, filters).Count()
                })
                .FirstOrDefaultAsync();
        }

        private int AllDataCollectorCount(ReportsFilter filters, int nationalSocietyId) =>
            _nyssContext.DataCollectors
                .FilterByArea(filters.Area)
                .FilterByType(filters.DataCollectorType)
                .FilterByNationalSociety(nationalSocietyId)
                .FilterByTrainingMode(filters.IsTraining)
                .FilterOnlyNotDeletedBefore(filters.StartDate)
                .Count();
    }
}
