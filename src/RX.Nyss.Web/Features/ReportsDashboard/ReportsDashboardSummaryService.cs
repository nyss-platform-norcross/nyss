using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.ReportsDashboard.Dto;

namespace RX.Nyss.Web.Features.ReportsDashboard
{
    public interface IReportsDashboardSummaryService
    {
        AlertsSummaryResponseDto AlertsSummary(IQueryable<Alert> alerts);
        DataCollectionPointsSummaryResponse DataCollectionPointsSummary(IQueryable<Report> reports);
    }

    public class ReportsDashboardSummaryService : IReportsDashboardSummaryService
    {
        public AlertsSummaryResponseDto AlertsSummary(IQueryable<Alert> alerts) =>
            new AlertsSummaryResponseDto
            {
                Escalated = alerts.Count(a => a.Status == AlertStatus.Escalated),
                Dismissed = alerts.Count(a => a.Status == AlertStatus.Dismissed),
                Closed = alerts.Count(a => a.Status == AlertStatus.Closed)
            };

        public DataCollectionPointsSummaryResponse DataCollectionPointsSummary(IQueryable<Report> reports)
        {
            var dataCollectionPointReports = reports.Where(r => r.DataCollectionPointCase != null);

            return new DataCollectionPointsSummaryResponse
            {
                FromOtherVillagesCount = dataCollectionPointReports.Sum(r => r.DataCollectionPointCase.FromOtherVillagesCount ?? 0),
                ReferredToHospitalCount = dataCollectionPointReports.Sum(r => r.DataCollectionPointCase.ReferredCount ?? 0),
                DeathCount = dataCollectionPointReports.Sum(r => r.DataCollectionPointCase.DeathCount ?? 0),
            };
        }
    }
}
