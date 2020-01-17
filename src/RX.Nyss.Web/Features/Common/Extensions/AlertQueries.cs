using System;
using System.Linq;

namespace RX.Nyss.Web.Features.Common.Extensions
{
    public static class AlertQueries
    {
        public static IQueryable<Nyss.Data.Models.Alert> FilterByDateAndStatus(this IQueryable<Nyss.Data.Models.Alert> alerts, DateTime startDate, DateTime endDate) =>
            alerts
                .Where(a => (a.ClosedAt != null && a.ClosedAt > startDate && a.ClosedAt <= endDate) ||
                    (a.DismissedAt != null && a.DismissedAt > startDate && a.DismissedAt <= endDate) ||
                    (a.EscalatedAt != null && a.EscalatedAt > startDate && a.EscalatedAt <= endDate));

        public static IQueryable<Nyss.Data.Models.Alert> FilterByProject(this IQueryable<Nyss.Data.Models.Alert> alerts, int projectId) =>
            alerts.Where(alert => alert.ProjectHealthRisk.Project.Id == projectId);
    }
}
