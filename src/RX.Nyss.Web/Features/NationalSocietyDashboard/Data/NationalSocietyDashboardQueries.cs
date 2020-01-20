using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.NationalSocietyDashboard.Dto;

namespace RX.Nyss.Web.Features.NationalSocietyDashboard.Data
{
    public static class NationalSocietyDashboardQueries
    {
        public static IQueryable<RawReport> GetAssignedRawReports(IQueryable<RawReport> reportsQueryable, int nationalSocietyId, NationalSocietyDashboardFiltersRequestDto filtersDto) =>
            reportsQueryable
                .Where(r => r.IsTraining.HasValue && r.IsTraining == filtersDto.IsTraining)
                .FromKnownDataCollector()
                .FilterByArea(filtersDto.Area)
                .FilterByDataCollectorType(MapToDataCollectorType(filtersDto.ReportsType))
                .FilterReportsByNationalSociety(nationalSocietyId)
                .FilterByDate(filtersDto.StartDate.Date, filtersDto.EndDate.Date.AddDays(1))
                .FilterByHealthRisk(filtersDto.HealthRiskId);

        public static IQueryable<Nyss.Data.Models.Report> GetValidReports(IQueryable<RawReport> reportsQueryable, int nationalSocietyId, NationalSocietyDashboardFiltersRequestDto filtersDto) =>
            GetAssignedRawReports(reportsQueryable, nationalSocietyId, filtersDto)
                .AllSuccessfulReports()
                .Select(r => r.Report)
                .Where(r => r.ProjectHealthRisk.HealthRisk.HealthRiskType != HealthRiskType.Activity)
                .Where(r => !r.MarkedAsError);

        public static DataCollectorType? MapToDataCollectorType(NationalSocietyDashboardFiltersRequestDto.ReportsTypeDto reportsType) =>
            reportsType switch
            {
                NationalSocietyDashboardFiltersRequestDto.ReportsTypeDto.DataCollector => DataCollectorType.Human,
                NationalSocietyDashboardFiltersRequestDto.ReportsTypeDto.DataCollectionPoint => DataCollectorType.CollectionPoint,
                _ => null as DataCollectorType?
            };
    }
}
