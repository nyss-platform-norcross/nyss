using System;
using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.Common.Extensions
{
    public static class RawReportQueries
    {
        public static IQueryable<RawReport> FilterByDataCollectorType(this IQueryable<RawReport> reports, DataCollectorType? dataCollectorType) =>
           dataCollectorType switch
           {
               DataCollectorType.Human =>
               reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.Human),

               DataCollectorType.CollectionPoint =>
               reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint),

               _ =>
               reports
           };

        public static IQueryable<RawReport> FilterReportsByNationalSociety(this IQueryable<RawReport> reports, int nationalSocietyId) =>
            reports.Where(r => r.Report.DataCollector.Project.NationalSocietyId == nationalSocietyId);

        public static IQueryable<RawReport> AllSuccessfulReports(this IQueryable<RawReport> reports) =>
            reports.Where(r => r.Report != null);

        public static IQueryable<RawReport> FilterByDate(this IQueryable<RawReport> reports, DateTime startDate, DateTime endDate) =>
            reports.Where(r => r.ReceivedAt >= startDate && r.ReceivedAt < endDate);

        public static IQueryable<RawReport> FilterByHealthRisk(this IQueryable<RawReport> reports, int? healthRiskId) =>
            reports.Where(r => !healthRiskId.HasValue || (r.Report != null && r.Report.ProjectHealthRisk.HealthRiskId == healthRiskId.Value));

        public static IQueryable<RawReport> FilterByProject(this IQueryable<RawReport> reports, int projectId) =>
            reports.Where(r => r.DataCollector.Project.Id == projectId);

        public static IQueryable<RawReport> FromKnownDataCollector(this IQueryable<RawReport> reports) =>
            reports.Where(r => r.DataCollector != null);

        public static IQueryable<RawReport> FilterByArea(this IQueryable<RawReport> reports, AreaDto area) =>
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

        public static IQueryable<RawReport> FilterByTrainingMode(this IQueryable<RawReport> rawReports, bool isTraining) =>
            rawReports.Where(r => r.IsTraining.HasValue && r.IsTraining == isTraining);
    }
}
