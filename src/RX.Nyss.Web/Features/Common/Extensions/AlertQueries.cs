using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Alerts.Dto;

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

        public static IQueryable<Alert> FilterByOrganization(this IQueryable<Alert> alerts, int? organizationId) =>
            organizationId.HasValue
                ? alerts.Where(alert => alert.AlertReports.Any(ar => ar.Report.DataCollector.Supervisor.UserNationalSocieties
                    .Any(uns =>
                        uns.NationalSociety == ar.Alert.ProjectHealthRisk.Project.NationalSociety
                        && uns.OrganizationId == organizationId.Value)))
                : alerts;

        public static IQueryable<Alert> FilterByHealthRisk(this IQueryable<Alert> alerts, int? healthRiskId) =>
            alerts.Where(a => !healthRiskId.HasValue || a.ProjectHealthRisk.HealthRiskId == healthRiskId.Value);

        public static IQueryable<Alert> FilterByHealthRisks(this IQueryable<Alert> alerts, List<int> healthRisks) =>
            healthRisks.Any()
                ? alerts.Where(a => healthRisks.Contains(a.ProjectHealthRisk.HealthRiskId))
                : alerts;

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

        public static IQueryable<Alert> FilterByStatus(this IQueryable<Alert> alerts, AlertStatusFilter status) =>
            status switch
            {
                AlertStatusFilter.All => alerts,
                AlertStatusFilter.Open => alerts.Where(a => a.Status == AlertStatus.Pending),
                AlertStatusFilter.Escalated => alerts.Where(a => a.Status == AlertStatus.Escalated),
                AlertStatusFilter.Dismissed => alerts.Where(a => a.Status == AlertStatus.Dismissed),
                AlertStatusFilter.Closed => alerts.Where(a => a.Status == AlertStatus.Closed),
                _ => alerts
            };

        public static IQueryable<Alert> Sort(this IQueryable<Alert> alerts, string orderBy, bool sortAscending) =>
            orderBy switch
            {
                AlertListFilterRequestDto.TimeTriggeredColumnName => sortAscending
                    ? alerts.OrderBy(a => a.CreatedAt)
                    : alerts.OrderByDescending(a => a.CreatedAt),
                AlertListFilterRequestDto.TimeOfLastReportColumnName => sortAscending
                    ? alerts.OrderBy(a => a.AlertReports.OrderByDescending(ar => ar.Report.ReceivedAt).First().Report.ReceivedAt)
                    : alerts.OrderByDescending(a => a.AlertReports.OrderByDescending(ar => ar.Report.ReceivedAt).First().Report.ReceivedAt),
                AlertListFilterRequestDto.StatusColumnName => sortAscending
                    ? alerts.OrderBy(a => a.Status == AlertStatus.Dismissed ? 0 :
                            a.Status == AlertStatus.Closed ? 1 :
                            a.Status == AlertStatus.Escalated ? 2 : 3)
                        .ThenByDescending(a => a.CreatedAt)
                    : alerts.OrderBy(a => a.Status == AlertStatus.Pending ? 0 :
                            a.Status == AlertStatus.Escalated ? 1 :
                            a.Status == AlertStatus.Closed ? 2 : 3)
                        .ThenByDescending(a => a.CreatedAt),
                _ => alerts
            };
    }
}
