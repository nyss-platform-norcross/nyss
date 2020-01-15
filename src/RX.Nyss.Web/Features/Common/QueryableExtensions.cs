using System;
using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.Common
{
    public static class ReportQueryableExtensions
    {
        public static IQueryable<Nyss.Data.Models.Report> FilterReportsByDate(this IQueryable<Nyss.Data.Models.Report> reports, DateTime startDate, DateTime endDate) =>
            reports.Where(r => r.ReceivedAt >= startDate && r.ReceivedAt < endDate);

        public static IQueryable<Nyss.Data.Models.Report> FilterReportsByProject(this IQueryable<Nyss.Data.Models.Report> reports, int projectId) =>
            reports.Where(r => r.DataCollector.Project.Id == projectId);

        public static IQueryable<Nyss.Data.Models.Report> FilterReportsByHealthRisk(this IQueryable<Nyss.Data.Models.Report> reports, int? healthRiskId) =>
            reports.Where(r => !healthRiskId.HasValue || r.ProjectHealthRisk.HealthRiskId == healthRiskId.Value);

        public static IQueryable<Nyss.Data.Models.Report> FilterReportsByRegion(this IQueryable<Nyss.Data.Models.Report> reports, AreaDto area) =>
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

        public static IQueryable<Nyss.Data.Models.Report> AllSuccessfulReports(this IQueryable<Nyss.Data.Models.RawReport> reports) =>
            reports.Where(r => r.Report != null).Select(r => r.Report);

        public static IQueryable<Nyss.Data.Models.RawReport> FilterReportsByDate(this IQueryable<Nyss.Data.Models.RawReport> reports, DateTime startDate, DateTime endDate) =>
            reports.Where(r => r.ReceivedAt >= startDate && r.ReceivedAt < endDate);

        public static IQueryable<Nyss.Data.Models.RawReport> FilterReportsByProject(this IQueryable<Nyss.Data.Models.RawReport> reports, int projectId) =>
            reports.Where(r => r.DataCollector.Project.Id == projectId);

        public static IQueryable<Nyss.Data.Models.RawReport> OnlyErrorReports(this IQueryable<Nyss.Data.Models.RawReport> reports) =>
            reports.Where(r => r.ReportId == null);

        public static IQueryable<Nyss.Data.Models.RawReport> FromKnownDataCollector(this IQueryable<Nyss.Data.Models.RawReport> reports) =>
            reports.Where(r => r.DataCollector.Id > 0);
    }

    public static class DataCollectorQueryableExtensions
    {

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterByDataCollectorType(this IQueryable<Nyss.Data.Models.DataCollector> reports, DataCollectorType? dataCollectorType) =>
            dataCollectorType switch
            {
                DataCollectorType.Human =>
                reports.Where(dc => dc.DataCollectorType == DataCollectorType.Human),

                DataCollectorType.CollectionPoint =>
                reports.Where(dc => dc.DataCollectorType == DataCollectorType.CollectionPoint),

                _ =>
                reports
            };

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterDataCollectorsByArea(this IQueryable<Nyss.Data.Models.DataCollector> dataCollectors, AreaDto area) =>
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

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterDataCollectorsByProject(this IQueryable<Nyss.Data.Models.DataCollector> reports, int projectId) =>
            reports.Where(dc => dc.Project.Id == projectId);

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterDataCollectorsByTrainingMode(this IQueryable<Nyss.Data.Models.DataCollector> reports, bool isInTraining) =>
            reports.Where(dc => (isInTraining ? dc.IsInTrainingMode : !dc.IsInTrainingMode));

        public static IQueryable<Nyss.Data.Models.DataCollector> FilterOnlyNotDeleted(this IQueryable<Nyss.Data.Models.DataCollector> reports) =>
            reports.Where(dc => !dc.DeletedAt.HasValue);
    }
}
