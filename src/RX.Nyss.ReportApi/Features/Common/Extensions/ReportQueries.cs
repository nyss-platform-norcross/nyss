using System;
using System.Collections.Generic;
using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.ReportApi.Features.Common.Extensions
{
    public static class ReportQueries
    {
        public static IQueryable<Report> OnlyRealReports(this IQueryable<Report> reports) =>
            reports.Where(r => !r.IsTraining);

        public static IQueryable<Report> FilterByReportStatus(this IQueryable<Report> reports, IEnumerable<ReportStatus> reportStatuses) =>
            reports.Where(r => reportStatuses.Contains(r.Status));

        public static IQueryable<Report> FilterByGroupLabel(this IQueryable<Report> reports, Guid reportGroupLabel) =>
            reports.Where(r => r.ReportGroupLabel == reportGroupLabel);

        public static IQueryable<Report> NotInExistingAlert(this IQueryable<Report> reports, int? alertIdToIgnore) =>
            reports.Where(r => !r.ReportAlerts.Any(ra => ra.Alert.Status == AlertStatus.Open || ra.Alert.Status == AlertStatus.Escalated || ra.Alert.Status == AlertStatus.Closed)
                || r.ReportAlerts.Any(ra => ra.AlertId == alertIdToIgnore));

        public static IQueryable<Report> OnlyReportsThatCanTriggerNewAlert(this IQueryable<Report> reports) =>
            reports.Where(r => !r.ReportAlerts.Any(ar => StatusConstants.AlertStatusesNotAllowingReportsToTriggerNewAlert.Contains(ar.Alert.Status)));
    }
}
