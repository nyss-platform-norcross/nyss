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

        public static IQueryable<RawReport> FilterByReportType(this IQueryable<RawReport> reports, ReportListType reportType) =>
            reportType switch
            {
                ReportListType.Main => reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.Human),
                ReportListType.FromDcp => reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint),
                ReportListType.UnknownSender => reports.Where(r => r.DataCollector == null),
                _ => reports
            };

        public static IQueryable<RawReport> FilterReportsByNationalSociety(this IQueryable<RawReport> reports, int? nationalSocietyId) =>
            reports.Where(r => !nationalSocietyId.HasValue || r.DataCollector.Project.NationalSocietyId == nationalSocietyId.Value);

        public static IQueryable<RawReport> FilterByOrganization(this IQueryable<RawReport> reports, int? organizationId) =>
            organizationId.HasValue
                ? reports.Where(r => r.DataCollector.Supervisor.UserNationalSocieties
                    .Any(uns =>
                        uns.NationalSociety == r.DataCollector.Project.NationalSociety
                        && uns.OrganizationId == organizationId.Value))
                : reports;

        public static IQueryable<RawReport> AllSuccessfulReports(this IQueryable<RawReport> reports) =>
            reports.Where(r => r.Report != null);

        public static IQueryable<RawReport> FilterByDate(this IQueryable<RawReport> reports, DateTimeOffset startDate, DateTimeOffset endDate) =>
            reports.Where(r => r.ReceivedAt >= startDate && r.ReceivedAt < endDate);

        public static IQueryable<RawReport> FilterByHealthRisk(this IQueryable<RawReport> reports, int? healthRiskId) =>
            reports.Where(r => !healthRiskId.HasValue || (r.Report != null && r.Report.ProjectHealthRisk.HealthRiskId == healthRiskId.Value));

        public static IQueryable<RawReport> FilterByProject(this IQueryable<RawReport> reports, int? projectId) =>
            reports.Where(r => !projectId.HasValue || r.DataCollector.Project.Id == projectId.Value);

        public static IQueryable<RawReport> FromKnownDataCollector(this IQueryable<RawReport> reports) =>
            reports.Where(r => r.DataCollector != null);

        public static IQueryable<RawReport> FilterByArea(this IQueryable<RawReport> reports, Area area) =>
            area?.AreaType switch
            {
                AreaType.Region =>
                    reports.Where(r => r.Village.District.Region.Id == area.AreaId),

                AreaType.District =>
                    reports.Where(r => r.Village.District.Id == area.AreaId),

                AreaType.Village =>
                    reports.Where(r => r.Village.Id == area.AreaId),

                AreaType.Zone =>
                    reports.Where(r => r.Zone.Id == area.AreaId),

                AreaType.Unknown =>
                    reports.Where(r => r.Village == null),

                _ => reports
            };

        public static IQueryable<RawReport> FilterByTrainingMode(this IQueryable<RawReport> rawReports, bool isTraining) =>
            rawReports.Where(r => r.IsTraining.HasValue && r.IsTraining == isTraining);
    }
}
