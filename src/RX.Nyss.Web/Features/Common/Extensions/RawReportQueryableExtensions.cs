using System;
using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.ProjectDashboard.Dto;

namespace RX.Nyss.Web.Features.Common.Extensions
{
    public static class RawReportQueryableExtensions
    {
        public static IQueryable<RawReport> FilterByRegion(this IQueryable<RawReport> reports, FiltersRequestDto.AreaDto area) =>
            area?.Type switch
            {
                FiltersRequestDto.AreaTypeDto.Region =>
                reports.Where(r => r.DataCollector.Village.District.Region.Id == area.Id),

                FiltersRequestDto.AreaTypeDto.District =>
                reports.Where(r => r.DataCollector.Village.District.Id == area.Id),

                FiltersRequestDto.AreaTypeDto.Village =>
                reports.Where(r => r.DataCollector.Village.Id == area.Id),

                FiltersRequestDto.AreaTypeDto.Zone =>
                reports.Where(r => r.DataCollector.Zone.Id == area.Id),

                _ =>
                reports
            };

        public static IQueryable<RawReport> FilterByDate(this IQueryable<RawReport> reports, DateTime startDate, DateTime endDate) =>
            reports.Where(r => r.ReceivedAt >= startDate && r.ReceivedAt < endDate);

        public static IQueryable<RawReport> FilterByProject(this IQueryable<RawReport> reports, int projectId) =>
            reports.Where(r => r.DataCollector.Project.Id == projectId);

        public static IQueryable<RawReport> FilterByTrainingStatus(this IQueryable<RawReport> reports, bool isTraining) =>
            reports.Where(r => isTraining ? r.IsTraining.Value : !r.IsTraining.Value);

        public static IQueryable<RawReport> FilterByHealthRisk(this IQueryable<RawReport> reports, int? healthRiskId) =>
            reports.Where(r => !healthRiskId.HasValue || r.Report.ProjectHealthRisk.HealthRiskId == healthRiskId.Value);

        public static IQueryable<RawReport> FilterOnlyErrorReports(this IQueryable<RawReport> reports) =>
            reports.Where(r => r.Report == null || r.Report.MarkedAsError);

        public static IQueryable<RawReport> FilterByDataCollectorType(this IQueryable<RawReport> reports, FiltersRequestDto.ReportsTypeDto reportsType) =>
            reportsType switch
            {
                FiltersRequestDto.ReportsTypeDto.DataCollector =>
                reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.Human),

                FiltersRequestDto.ReportsTypeDto.DataCollectionPoint =>
                reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint),

                _ =>
                reports
            };
    }
}
