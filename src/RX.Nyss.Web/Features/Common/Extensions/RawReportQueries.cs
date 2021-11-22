using System;
using System.Collections.Generic;
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

        public static IQueryable<RawReport> FilterByDataCollectorType(this IQueryable<RawReport> reports, ReportListDataCollectorType reportDataCollectorType) =>
            reportDataCollectorType switch
            {
                ReportListDataCollectorType.Human => reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.Human),
                ReportListDataCollectorType.CollectionPoint => reports.Where(r => r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint),
                ReportListDataCollectorType.UnknownSender => reports.Where(r => r.DataCollector == null),
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

        public static IQueryable<RawReport> FilterByHealthRisks(this IQueryable<RawReport> reports, IList<int> healthRiskIds) =>
            healthRiskIds != null && healthRiskIds.Any()
                ? reports.Where(r => healthRiskIds.Contains(r.Report.ProjectHealthRisk.HealthRiskId))
                : reports;

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

        public static IQueryable<RawReport> FilterByFormatCorrectness(this IQueryable<RawReport> rawReports, bool formatCorrect) =>
        formatCorrect
            ? rawReports.Where(r => r.Report != null && !r.Report.MarkedAsError)
            : rawReports.Where(r => r.Report == null || r.Report.MarkedAsError);

        public static IQueryable<RawReport> FilterByErrorType(this IQueryable<RawReport> rawReports, ReportErrorFilterType? reportErrorFilterType) =>
            reportErrorFilterType switch
            {
                ReportErrorFilterType.All => rawReports.Where(r => r.ErrorType.HasValue),
                ReportErrorFilterType.HealthRiskNotFound => rawReports.Where(r => r.ErrorType == ReportErrorType.HealthRiskNotFound || r.ErrorType == ReportErrorType.GlobalHealthRiskCodeNotFound),
                ReportErrorFilterType.WrongFormat => rawReports
                    .Where(r => r.ErrorType == ReportErrorType.FormatError
                        || r.ErrorType == ReportErrorType.EventReportHumanHealthRisk
                        || r.ErrorType == ReportErrorType.AggregateReportNonHumanHealthRisk
                        || r.ErrorType == ReportErrorType.CollectionPointNonHumanHealthRisk
                        || r.ErrorType == ReportErrorType.CollectionPointUsedDataCollectorFormat
                        || r.ErrorType == ReportErrorType.DataCollectorUsedCollectionPointFormat
                        || r.ErrorType == ReportErrorType.SingleReportNonHumanHealthRisk
                        || r.ErrorType == ReportErrorType.GenderAndAgeNonHumanHealthRisk),
                ReportErrorFilterType.GatewayError => rawReports.Where(r => r.ErrorType == ReportErrorType.Gateway),
                ReportErrorFilterType.Other => rawReports.Where(r => r.ErrorType == ReportErrorType.Other),
                _ => rawReports
            };

        public static IQueryable<RawReport> FilterByReportStatus(this IQueryable<RawReport> rawReports, ReportStatusFilterDto filterDto) =>
            filterDto != null
                ? rawReports.Where(r => (filterDto.Kept && r.Report != null && r.Report.Status == ReportStatus.Accepted)
                    || (filterDto.Dismissed && r.Report != null && r.Report.Status == ReportStatus.Rejected)
                    || (filterDto.NotCrossChecked && r.Report != null && (r.Report.Status == ReportStatus.New || r.Report.Status == ReportStatus.Pending || r.Report.Status == ReportStatus.Closed)))
                : rawReports;

        public static IQueryable<RawReport> FilterByTrainingMode(this IQueryable<RawReport> rawReports, TrainingStatusDto? trainingStatus) =>
            trainingStatus switch
            {
                TrainingStatusDto.InTraining => rawReports.Where(r => r.IsTraining.HasValue && r.IsTraining.Value),
                TrainingStatusDto.Trained => rawReports.Where(r => r.IsTraining.HasValue && !r.IsTraining.Value),
                _ => rawReports
            };
    }
}
