using System;
using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.ProjectDashboard.Dto;

namespace RX.Nyss.Web.Features.Common.Extensions
{
    public static class ReportQueryableExtensions
    {
        public static IQueryable<Nyss.Data.Models.Report> FilterByRegion(this IQueryable<Nyss.Data.Models.Report> reports, FiltersRequestDto.AreaDto area) =>
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


        public static IQueryable<Nyss.Data.Models.Report> FilterByDate(this IQueryable<Nyss.Data.Models.Report> reports, DateTime startDate, DateTime endDate) =>
            reports.Where(r => r.ReceivedAt >= startDate && r.ReceivedAt < endDate);

        public static IQueryable<Nyss.Data.Models.Report> FilterByTrainingStatus(this IQueryable<Nyss.Data.Models.Report> reports, bool isTraining) =>
            reports.Where(r => isTraining ? r.IsTraining : !r.IsTraining);

        public static IQueryable<Nyss.Data.Models.Report> FilterOnlyNotError(this IQueryable<Nyss.Data.Models.Report> reports) =>
            reports.Where(r => !r.MarkedAsError);

        public static IQueryable<Nyss.Data.Models.Report> FilterByProject(this IQueryable<Nyss.Data.Models.Report> reports, int projectId) =>
            reports.Where(r => r.DataCollector.Project.Id == projectId);

        public static IQueryable<Nyss.Data.Models.Report> FilterReportsByHealthRisk(this IQueryable<Nyss.Data.Models.Report> reports, int? healthRiskId) =>
            reports.Where(r => !healthRiskId.HasValue || r.ProjectHealthRisk.HealthRiskId == healthRiskId.Value);

        public static IQueryable<Nyss.Data.Models.Report> FilterByDataCollectorType(this IQueryable<Nyss.Data.Models.Report> reports, FiltersRequestDto.ReportsTypeDto reportsType) =>
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
