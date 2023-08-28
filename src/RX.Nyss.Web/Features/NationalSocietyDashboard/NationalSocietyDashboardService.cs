using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.NationalSocieties;
using RX.Nyss.Web.Features.NationalSocietyDashboard.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Services.ReportsDashboard;
using RX.Nyss.Web.Services.ReportsDashboard.Dto;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard
{
    public interface INationalSocietyDashboardService
    {
        Task<Result<NationalSocietyDashboardFiltersResponseDto>> GetFiltersData(int nationalSocietyId);

        Task<Result<NationalSocietyDashboardResponseDto>> GetData(int nationalSocietyId, NationalSocietyDashboardFiltersRequestDto filtersDto);

        Task<Result<IEnumerable<ReportsSummaryHealthRiskResponseDto>>> GetReportHealthRisks(int nationalSocietyId, double latitude, double longitude,
            NationalSocietyDashboardFiltersRequestDto filtersDto);
    }

    public class NationalSocietyDashboardService : INationalSocietyDashboardService
    {
        private readonly INationalSocietyService _nationalSocietyService;
        private readonly INationalSocietyDashboardSummaryService _nationalSocietyDashboardSummaryService;
        private readonly IReportsDashboardMapService _reportsDashboardMapService;
        private readonly IReportsDashboardByVillageService _reportsDashboardByVillageService;
        private readonly INyssContext _nyssContext;
        private readonly INationalSocietyStructureService _nationalSocietyStructureService;

        public NationalSocietyDashboardService(
            INationalSocietyService nationalSocietyService,
            INationalSocietyDashboardSummaryService nationalSocietyDashboardSummaryService,
            IReportsDashboardMapService reportsDashboardMapService,
            IReportsDashboardByVillageService reportsDashboardByVillageService,
            INyssContext nyssContext,
            INationalSocietyStructureService nationalSocietyStructureService)
        {
            _nationalSocietyService = nationalSocietyService;
            _nationalSocietyDashboardSummaryService = nationalSocietyDashboardSummaryService;
            _reportsDashboardMapService = reportsDashboardMapService;
            _reportsDashboardByVillageService = reportsDashboardByVillageService;
            _nyssContext = nyssContext;
            _nationalSocietyStructureService = nationalSocietyStructureService;
        }

        public async Task<Result<NationalSocietyDashboardFiltersResponseDto>> GetFiltersData(int nationalSocietyId)
        {
            var healthRiskNames = await _nationalSocietyService.GetHealthRiskNames(nationalSocietyId, true);

            var organizations = await GetOrganizations(nationalSocietyId);

            var locationStructure = await _nationalSocietyStructureService.Get(nationalSocietyId);

            var dto = new NationalSocietyDashboardFiltersResponseDto
            {
                HealthRisks = healthRiskNames,
                Organizations = organizations,
                Locations = locationStructure
            };

            return Success(dto);
        }

        public async Task<Result<NationalSocietyDashboardResponseDto>> GetData(int nationalSocietyId, NationalSocietyDashboardFiltersRequestDto filtersDto)
        {
            if (filtersDto.EndDate < filtersDto.StartDate)
            {
                return Success(new NationalSocietyDashboardResponseDto());
            }

            var epiWeekStartDay = await _nyssContext.NationalSocieties
                .Where(ns => ns.Id == nationalSocietyId)
                .Select(ns => ns.EpiWeekStartDay)
                .SingleAsync();

            var filters = MapToReportFilters(nationalSocietyId, filtersDto);
            var reportsGroupedByVillageAndDate = await _reportsDashboardByVillageService.GetReportsGroupedByVillageAndDate(filters, filtersDto.GroupingType, epiWeekStartDay);

            var dashboardDataDto = new NationalSocietyDashboardResponseDto
            {
                Summary = await _nationalSocietyDashboardSummaryService.GetData(filters),
                ReportsGroupedByLocation = await _reportsDashboardMapService.GetProjectSummaryMap(filters),
                ReportsGroupedByVillageAndDate = reportsGroupedByVillageAndDate,
                ReportsGroupedByHealthRiskAndDate = await _reportsDashboardByHealthRiskService.GetReportsGroupedByHealthRiskAndDate(filters, filtersDto.GroupingType, epiWeekStartDay),
            };

            return Success(dashboardDataDto);
        }

        public async Task<Result<IEnumerable<ReportsSummaryHealthRiskResponseDto>>> GetReportHealthRisks(int nationalSocietyId, double latitude, double longitude,
            NationalSocietyDashboardFiltersRequestDto filtersDto)
        {
            var filters = MapToReportFilters(nationalSocietyId, filtersDto);
            var data = await _reportsDashboardMapService.GetProjectReportHealthRisks(filters, latitude, longitude);
            return Success(data);
        }

        private async Task<List<NationalSocietyDashboardFiltersResponseDto.OrganizationDto>> GetOrganizations(int nationalSocietyId) =>
            await _nyssContext.NationalSocieties
                .Where(ns => ns.Id == nationalSocietyId)
                .SelectMany(p => p.Organizations)
                .Select(o => new NationalSocietyDashboardFiltersResponseDto.OrganizationDto
                {
                    Id = o.Id,
                    Name = o.Name
                }).ToListAsync();

        private ReportsFilter MapToReportFilters(int nationalSocietyId, NationalSocietyDashboardFiltersRequestDto filtersDto) =>
            new ReportsFilter
            {
                StartDate = filtersDto.StartDate,
                EndDate = filtersDto.EndDate.AddDays(1),
                HealthRisks = filtersDto.HealthRisks.ToList(),
                NationalSocietyId = nationalSocietyId,
                OrganizationId = filtersDto.OrganizationId,
                Area = filtersDto.Locations,
                DataCollectorType = MapToDataCollectorType(filtersDto.DataCollectorType),
                ReportStatus = filtersDto.ReportStatus,
                TrainingStatus = TrainingStatusDto.Trained,
                UtcOffset = filtersDto.UtcOffset
            };

        private static DataCollectorType? MapToDataCollectorType(NationalSocietyDashboardFiltersRequestDto.NationalSocietyDataCollectorTypeDto nationalSocietyDataCollectorType) =>
            nationalSocietyDataCollectorType switch
            {
                NationalSocietyDashboardFiltersRequestDto.NationalSocietyDataCollectorTypeDto.DataCollector => DataCollectorType.Human,
                NationalSocietyDashboardFiltersRequestDto.NationalSocietyDataCollectorTypeDto.DataCollectionPoint => DataCollectorType.CollectionPoint,
                _ => null as DataCollectorType?
            };
    }
}
