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

        public static IQueryable<Alert> FilterByProject(this IQueryable<Alert> alerts, int projectId) =>
            alerts.Where(alert => alert.ProjectHealthRisk.Project.Id == projectId);

        public static IQueryable<Alert> FilterByNationalSociety(this IQueryable<Alert> alerts, int nationalSocietyId) =>
            alerts.Where(alert => alert.ProjectHealthRisk.Project.NationalSocietyId == nationalSocietyId);
    }
}
