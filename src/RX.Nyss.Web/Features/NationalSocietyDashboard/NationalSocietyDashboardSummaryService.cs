using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
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
            if (filters.NationalSocietyId is null)
            {
                throw new InvalidOperationException("NationalSocietyId was not supplied");
            }

            var nationalSocietyId = filters.NationalSocietyId.Value;

            // Check existence first (cheap, indexed lookup)
            var exists = await _nyssContext.NationalSocieties
                .AsNoTracking()
                .AnyAsync(ns => ns.Id == nationalSocietyId);

            if (!exists)
            {
                throw new InvalidOperationException($"NationalSociety with id {nationalSocietyId} does not exist");
            }

            // Base queries (keep IQueryable)
            var dashboardReportsQuery =
                _reportService.GetDashboardHealthRiskEventReportsQuery(filters);

            var rawReportsQuery =
                _reportService.GetRawReportsWithDataCollectorAndActivityReportsQuery(filters);

            // Materialize ONLY what must be in memory
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

            return new NationalSocietySummaryResponseDto
            {
                TotalReportCount =
                    dashboardReports.Sum(r => r.ReportedCaseCount),

                ActiveDataCollectorCount =
                    rawReports
                        .Select(r => r.DataCollectorId)
                        .OfType<int>()
                        .Distinct()
                        .Count(),

                // Keep these server-side
                DataCollectionPointSummary =
                    _reportsDashboardSummaryService
                        .DataCollectionPointsSummary(dashboardReportsQuery),

                AlertsSummary =
                    _reportsDashboardSummaryService
                        .AlertsSummary(filters),

                NumberOfDistricts =
                    rawReports
                        .Select(r => r.DistrictId)
                        .OfType<int>()
                        .Distinct()
                        .Count(),

                NumberOfVillages =
                    rawReports
                        .Select(r => r.VillageId)
                        .OfType<int>()
                        .Distinct()
                        .Count()
            };
        }

    }
}
