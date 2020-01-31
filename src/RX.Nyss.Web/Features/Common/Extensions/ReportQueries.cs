using System;
using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.Common.Extensions
{
    public static class ReportQueries
    {
        public static IQueryable<Report> FilterByNationalSociety(this IQueryable<Report> reports, int nationalSocietyId) =>
            reports.Where(r => r.DataCollector.Project.NationalSocietyId == nationalSocietyId);

        public static IQueryable<Report> FilterByDataCollectorType(this IQueryable<Report> reports, DataCollectorType? dataCollectorType) =>
            dataCollectorType switch
            {
                DataCollectorType.Human =>
                reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.Human),

                DataCollectorType.CollectionPoint =>
                reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint),

                _ =>
                reports
            };

        public static IQueryable<Report> FilterByDate(this IQueryable<Report> reports, DateTime startDate, DateTime endDate) =>
            reports.Where(r => r.ReceivedAt >= startDate && r.ReceivedAt < endDate);

        public static IQueryable<Report> FilterByProject(this IQueryable<Report> reports, int projectId) =>
            reports.Where(r => r.DataCollector.Project.Id == projectId);

        public static IQueryable<Report> FilterByHealthRisk(this IQueryable<Report> reports, int? healthRiskId) =>
            reports.Where(r => !healthRiskId.HasValue || r.ProjectHealthRisk.HealthRiskId == healthRiskId.Value);

        public static IQueryable<Report> FilterByArea(this IQueryable<Report> reports, AreaDto area) =>
            area?.Type switch
            {
                AreaType.Region =>
                reports.Where(r => r.RawReport.Village.District.Region.Id == area.Id),

                AreaType.District =>
                reports.Where(r => r.RawReport.Village.District.Id == area.Id),

                AreaType.Village =>
                reports.Where(r => r.RawReport.Village.Id == area.Id),

                AreaType.Zone =>
                reports.Where(r => r.RawReport.Zone.Id == area.Id),

                _ =>
                reports
            };
    }
}
