using System;
using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.Common
{
    public static class ReportQueryableExtensions
    {
        public static IQueryable<Nyss.Data.Models.Report> FilterByDate(this IQueryable<Nyss.Data.Models.Report> reports, DateTime startDate, DateTime endDate) =>
            reports.Where(r => r.ReceivedAt >= startDate && r.ReceivedAt < endDate);

        public static IQueryable<Nyss.Data.Models.Report> FilterByProject(this IQueryable<Nyss.Data.Models.Report> reports, int projectId) =>
            reports.Where(r => r.DataCollector.Project.Id == projectId);

        public static IQueryable<Nyss.Data.Models.Report> FilterByHealthRisk(this IQueryable<Nyss.Data.Models.Report> reports, int? healthRiskId) =>
            reports.Where(r => !healthRiskId.HasValue || r.ProjectHealthRisk.HealthRiskId == healthRiskId.Value);

        public static IQueryable<Nyss.Data.Models.Report> FilterByArea(this IQueryable<Nyss.Data.Models.Report> reports, AreaDto area) =>
            area?.Type switch
            {
                AreaDto.AreaType.Region =>
                reports.Where(r => r.Village.District.Region.Id == area.Id),

                AreaDto.AreaType.District =>
                reports.Where(r => r.Village.District.Id == area.Id),

                AreaDto.AreaType.Village =>
                reports.Where(r => r.Village.Id == area.Id),

                AreaDto.AreaType.Zone =>
                reports.Where(r => r.Zone.Id == area.Id),

                _ =>
                reports
            };
    }

    public static class RawReportQueryableExtensions
    {
        public static IQueryable<Nyss.Data.Models.RawReport> FilterByDataCollectorType(this IQueryable<Nyss.Data.Models.RawReport> reports, DataCollectorType? dataCollectorType) =>
            dataCollectorType switch
            {
                DataCollectorType.Human =>
                reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.Human),

                DataCollectorType.CollectionPoint =>
                reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint),

                _ =>
                reports
            };

        public static IQueryable<Nyss.Data.Models.RawReport> AllSuccessfulReports(this IQueryable<Nyss.Data.Models.RawReport> reports) =>
            reports.Where(r => r.Report != null);

        public static IQueryable<Nyss.Data.Models.RawReport> FilterByDate(this IQueryable<Nyss.Data.Models.RawReport> reports, DateTime startDate, DateTime endDate) =>
            reports.Where(r => r.ReceivedAt >= startDate && r.ReceivedAt < endDate);

        public static IQueryable<Nyss.Data.Models.RawReport> FilterByProject(this IQueryable<Nyss.Data.Models.RawReport> reports, int projectId) =>
            reports.Where(r => r.DataCollector.Project.Id == projectId);

        public static IQueryable<Nyss.Data.Models.RawReport> FromKnownDataCollector(this IQueryable<Nyss.Data.Models.RawReport> reports) =>
            reports.Where(r => r.DataCollector != null);

        public static IQueryable<Nyss.Data.Models.RawReport> FilterByArea(this IQueryable<Nyss.Data.Models.RawReport> reports, AreaDto area) =>
            area?.Type switch
            {
                AreaDto.AreaType.Region =>
                reports.Where(r => r.Report != null ? r.Report.Village.District.Region.Id == area.Id : r.DataCollector.Village.District.Region.Id == area.Id),

                AreaDto.AreaType.District =>
                reports.Where(r => r.Report != null ? r.Report.Village.District.Id == area.Id : r.DataCollector.Village.District.Id == area.Id),

                AreaDto.AreaType.Village =>
                reports.Where(r => r.Report != null ? r.Report.Village.Id == area.Id : r.DataCollector.Village.Id == area.Id),

                AreaDto.AreaType.Zone =>
                reports.Where(r => r.Report != null ? r.Report.Zone.Id == area.Id : r.DataCollector.Zone.Id == area.Id),

                _ =>
                reports
            };
    }

    public static class DataCollectorQueryableExtensions
    {
        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByType(this IQueryable<Nyss.Data.Models.DataCollector> reports, DataCollectorType? dataCollectorType) =>
            dataCollectorType switch
            {
                DataCollectorType.Human =>
                reports.Where(dc => dc.DataCollectorType == DataCollectorType.Human),

                DataCollectorType.CollectionPoint =>
                reports.Where(dc => dc.DataCollectorType == DataCollectorType.CollectionPoint),

                _ =>
                reports
            };

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByArea(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors, AreaDto area) =>
            area?.Type switch
            {
                AreaDto.AreaType.Region =>
                dataCollectors.Where(dc => dc.Village.District.Region.Id == area.Id),

                AreaDto.AreaType.District =>
                dataCollectors.Where(dc => dc.Village.District.Id == area.Id),

                AreaDto.AreaType.Village =>
                dataCollectors.Where(dc => dc.Village.Id == area.Id),

                AreaDto.AreaType.Zone =>
                dataCollectors.Where(dc => dc.Zone.Id == area.Id),

                _ =>
                dataCollectors
            };

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByProject(this IQueryable<Nyss.Data.Models.DataCollector> reports, int projectId) =>
            reports.Where(dc => dc.Project.Id == projectId);

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByTrainingMode(this IQueryable<Nyss.Data.Models.DataCollector> reports, bool isInTraining) =>
            reports.Where(dc => (isInTraining ? dc.IsInTrainingMode : !dc.IsInTrainingMode));

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterOnlyNotDeletedBefore(this IQueryable<Nyss.Data.Models.DataCollector> reports, DateTime startDate) =>
            reports.Where(dc => dc.DeletedAt == null || dc.DeletedAt > startDate);
    }
}
