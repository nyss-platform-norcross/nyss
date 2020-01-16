using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.NationalSocietyDashboard.Dto;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard.Data
{
    public interface INationalSocietyDashboardReportsMapService
    {
        Task<IEnumerable<NationalSocietySummaryMapResponseDto>> GetSummaryMap(int nationalSocietyId, NationalSocietyDashboardFiltersRequestDto filtersDto);

        Task<IEnumerable<NationalSocietySummaryReportHealthRiskResponseDto>> GetLocationHealthRisks(int nationalSocietyId, double latitude, double longitude, NationalSocietyDashboardFiltersRequestDto filters);
    }

    public class NationalSocietyDashboardReportsMapService : INationalSocietyDashboardReportsMapService
    {
        private readonly INyssContext _nyssContext;

        public NationalSocietyDashboardReportsMapService(INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        public async Task<IEnumerable<NationalSocietySummaryMapResponseDto>> GetSummaryMap(int nationalSocietyId, NationalSocietyDashboardFiltersRequestDto filtersDto) =>
            await NationalSocietyDashboardQueries.GetValidReports(_nyssContext.RawReports, nationalSocietyId, filtersDto)
                .GroupBy(report => new { report.Location.X, report.Location.Y })
                .Select(grouping => new NationalSocietySummaryMapResponseDto
                {
                    ReportsCount = grouping.Sum(g => g.ReportedCaseCount),
                    Location = new NationalSocietySummaryMapResponseDto.MapReportLocation
                    {
                        Latitude = grouping.Key.Y,
                        Longitude = grouping.Key.X
                    }
                })
                .ToListAsync();

        public async Task<IEnumerable<NationalSocietySummaryReportHealthRiskResponseDto>> GetLocationHealthRisks(int nationalSocietyId, double latitude, double longitude, NationalSocietyDashboardFiltersRequestDto filters) =>
            await NationalSocietyDashboardQueries.GetValidReports(_nyssContext.RawReports, nationalSocietyId, filters)
                .Where(r => r.Location.X == longitude && r.Location.Y == latitude)
                .Select(r => new
                {
                    r.ProjectHealthRisk.HealthRiskId,
                    HealthRiskName = r.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.Id == r.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.Id)
                        .Select(lc => lc.Name).FirstOrDefault(),
                    Total = r.ReportedCaseCount,
                })
                .Where(r => r.Total > 0)
                .GroupBy(r => r.HealthRiskId)
                .Select(grouping => new NationalSocietySummaryReportHealthRiskResponseDto
                {
                    Name = _nyssContext.HealthRiskLanguageContents.Where(f => f.HealthRisk.Id == grouping.Key).Select(s => s.Name).FirstOrDefault(),
                    Value = grouping.Sum(r => r.Total),
                })
                .ToListAsync();
    }
}
