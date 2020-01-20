using System.Threading.Tasks;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Features.NationalSocieties;
using RX.Nyss.Web.Features.NationalSocietyDashboard.Dto;
using RX.Nyss.Web.Features.NationalSocietyDashboard.Data;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard
{
    public interface INationalSocietyDashboardService
    {
        Task<Result<NationalSocietyDashboardFiltersResponseDto>> GetDashboardFiltersData(int nationalSocietyId);
        
        Task<Result<NationalSocietyDashboardResponseDto>> GetDashboardData(int nationalSocietyId, NationalSocietyDashboardFiltersRequestDto filtersDto);
    }

    public class NationalSocietyDashboardService : INationalSocietyDashboardService
    {
        private readonly INationalSocietyService _nationalSocietyService;
        private readonly INationalSocietyDashboardReportsMapService _nationalSocietyDashboardReportsMapService;
        private readonly INationalSocietyDashboardSummaryService _nationalSocietyDashboardSummaryService;

        public NationalSocietyDashboardService(
            INationalSocietyService nationalSocietyService,
            INationalSocietyDashboardReportsMapService nationalSocietyDashboardReportsMapService,
            INationalSocietyDashboardSummaryService nationalSocietyDashboardSummaryService)
        {
            _nationalSocietyService = nationalSocietyService;
            _nationalSocietyDashboardReportsMapService = nationalSocietyDashboardReportsMapService;
            _nationalSocietyDashboardSummaryService = nationalSocietyDashboardSummaryService;
        }

        public async Task<Result<NationalSocietyDashboardFiltersResponseDto>> GetDashboardFiltersData(int nationalSocietyId)
        {
            var projectHealthRiskNames = await _nationalSocietyService.GetNationalSocietyHealthRiskNames(nationalSocietyId);

            var dto = new NationalSocietyDashboardFiltersResponseDto
            {
                HealthRisks = projectHealthRiskNames
            };

            return Success(dto);
        }

        public async Task<Result<NationalSocietyDashboardResponseDto>> GetDashboardData(int nationalSocietyId, NationalSocietyDashboardFiltersRequestDto filtersDto)
        {
            var projectSummary = await _nationalSocietyDashboardSummaryService.GetSummaryData(nationalSocietyId, filtersDto);
            var reportsGroupedByLocation = await _nationalSocietyDashboardReportsMapService.GetSummaryMap(nationalSocietyId, filtersDto);

            var dashboardDataDto = new NationalSocietyDashboardResponseDto
            {
                Summary = projectSummary,
                ReportsGroupedByLocation = reportsGroupedByLocation,
            };

            return Success(dashboardDataDto);
        }
    }
}
