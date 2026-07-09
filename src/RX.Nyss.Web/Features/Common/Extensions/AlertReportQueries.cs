using System;
using System.Collections.Generic;
using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.Common.Extensions;

public static class AlertReportQueries
{
    public static IQueryable<AlertReport> FilterByDate(this IQueryable<AlertReport> alertReports, DateTime startDate, DateTime endDate) =>
        alertReports
            .Where(ar => ar.Report.ReceivedAt >= startDate && ar.Report.ReceivedAt < endDate);

    public static IQueryable<AlertReport> FilterByHealthRisks(this IQueryable<AlertReport> alertReports, IList<int> healthRiskIds) =>
        healthRiskIds != null && healthRiskIds.Any()
            ? alertReports.Where(ar => healthRiskIds.Contains(ar.Report.ProjectHealthRisk.HealthRiskId))
            : alertReports;

    public static IQueryable<AlertReport> FilterByArea(this IQueryable<AlertReport> alertReports, AreaDto area) =>
        area != null && area.RegionIds.Any()
            ? alertReports.Where(ar => (area.RegionIds.Contains(ar.Report.RawReport.Village.District.Region.Id)
                && area.DistrictIds.Contains(ar.Report.RawReport.Village.District.Id)
                && area.VillageIds.Contains(ar.Report.RawReport.Village.Id)
                && (ar.Report.RawReport.Zone == null || area.ZoneIds.Contains(ar.Report.RawReport.Zone.Id)))
                || (area.IncludeUnknownLocation && ar.Report.RawReport.Village == null))
            : alertReports;

    public static IQueryable<AlertReport> FilterByReportStatus(this IQueryable<AlertReport> alertReports, ReportStatusFilterDto filterDto) =>
        filterDto != null
            ? alertReports.Where(ar => (filterDto.Kept && ar.Report != null && ar.Report.Status == ReportStatus.Accepted)
                || (filterDto.Dismissed && ar.Report != null && ar.Report.Status == ReportStatus.Rejected)
                || (filterDto.NotCrossChecked && ar.Report != null && (ar.Report.Status == ReportStatus.New || ar.Report.Status == ReportStatus.Pending || ar.Report.Status == ReportStatus.Closed)))
            : alertReports;

    public static IQueryable<AlertReport> AllReportsKeptBeforeAlertWasEscalated(this IQueryable<AlertReport> alertReports) =>
        alertReports.Where(ar => ar.Report.AcceptedAt < ar.Alert.EscalatedAt || ar.Report.RejectedAt < ar.Alert.EscalatedAt);

}
