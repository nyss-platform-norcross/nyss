using System.Linq;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.Reports;
using RX.Nyss.Web.Services.ReportsDashboard.Dto;

namespace RX.Nyss.Web.Services.ReportsDashboard
{
    public interface IReportsDashboardSummaryService
    {
        AlertsSummaryResponseDto AlertsSummary(ReportsFilter filter);
        DataCollectionPointsSummaryResponse DataCollectionPointsSummary(IQueryable<Report> reports);
    }

    public class ReportsDashboardSummaryService : IReportsDashboardSummaryService
    {
        private readonly INyssContext _nyssContext;

        public ReportsDashboardSummaryService(
            INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        public AlertsSummaryResponseDto AlertsSummary(ReportsFilter filter)
        {
            var alerts = filter.TrainingStatus == TrainingStatusDto.Trained
                ? GetAlerts(filter)
                : Enumerable.Empty<Alert>().AsQueryable();

            return new AlertsSummaryResponseDto
            {
                Escalated = alerts.Count(a => a.Status == AlertStatus.Escalated && a.EscalatedAt.HasValue && a.EscalatedAt >= filter.StartDate && a.EscalatedAt <= filter.EndDate),
                Dismissed = alerts.Count(a => a.DismissedAt.HasValue && a.DismissedAt >= filter.StartDate && a.DismissedAt <= filter.EndDate),
                Closed = alerts.Count(a => a.ClosedAt.HasValue && a.ClosedAt >= filter.StartDate && a.ClosedAt <= filter.EndDate),
                Open = alerts.Count(a => a.Status == AlertStatus.Open)
            };
        }

        public DataCollectionPointsSummaryResponse DataCollectionPointsSummary(IQueryable<Report> reports)
        {
            var dataCollectionPointReports = reports.Where(r => r.DataCollectionPointCase != null);

            return new DataCollectionPointsSummaryResponse
            {
                ReferredToHospitalCount = dataCollectionPointReports.Sum(r => r.DataCollectionPointCase.ReferredCount ?? 0)
            };
        }

        private IQueryable<Alert> GetAlerts(ReportsFilter filters) =>
            _nyssContext.Alerts
                .FilterByProject(filters.ProjectId)
                .FilterByNationalSociety(filters.NationalSocietyId)
                .FilterByOrganization(filters.OrganizationId)
                .FilterByHealthRisks(filters.HealthRisks)
                .FilterByArea(filters.Area);
    }
}
