using System;
using System.Linq;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.Common.Extensions
{
    public static class AlertQueries
    {
        public static IQueryable<Alert> FilterByDateAndStatus(this IQueryable<Alert> alerts, DateTime startDate, DateTime endDate) =>
            alerts
                .Where(a => (a.ClosedAt != null && a.ClosedAt > startDate && a.ClosedAt <= endDate) ||
                    (a.DismissedAt != null && a.DismissedAt > startDate && a.DismissedAt <= endDate) ||
                    (a.EscalatedAt != null && a.EscalatedAt > startDate && a.EscalatedAt <= endDate));

        public static IQueryable<Alert> FilterByProject(this IQueryable<Alert> alerts, int? projectId) =>
            alerts.Where(alert => !projectId.HasValue || alert.ProjectHealthRisk.Project.Id == projectId.Value);

        public static IQueryable<Alert> FilterByNationalSociety(this IQueryable<Alert> alerts, int? nationalSocietyId) =>
            alerts.Where(alert => !nationalSocietyId.HasValue || alert.ProjectHealthRisk.Project.NationalSocietyId == nationalSocietyId.Value);

        public static IQueryable<Alert> FilterByHealthRisk(this IQueryable<Alert> alerts, int? healthRiskId) =>
            alerts.Where(a => !healthRiskId.HasValue || a.ProjectHealthRisk.HealthRiskId == healthRiskId.Value);

        public static IQueryable<Alert> FilterByArea(this IQueryable<Alert> alerts, Area area) =>
            area?.AreaType switch
            {
                AreaType.Region =>
                alerts.Where(a => a.AlertReports.Any(ar => ar.Report.RawReport.Village.District.Region.Id == area.AreaId)),

                AreaType.District =>
                alerts.Where(a => a.AlertReports.Any(ar => ar.Report.RawReport.Village.District.Id == area.AreaId)),

                AreaType.Village =>
                alerts.Where(a => a.AlertReports.Any(ar => ar.Report.RawReport.Village.Id == area.AreaId)),

                AreaType.Zone =>
                alerts.Where(a => a.AlertReports.Any(ar => ar.Report.RawReport.Zone.Id == area.AreaId)),

                _ =>
                alerts
            };
    }
}
