using System.Collections.Generic;
using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.TestData.TestDataGeneration
{
    public class AlertGenerator
    {
        private readonly EntityNumerator _numerator = new EntityNumerator();

        public (List<Alert>, List<AlertReport>) AddPendingAlertForReports(List<Report> reports)
        {
            var projectHealthRisk = reports.First().ProjectHealthRisk;

            var newAlert = new Alert
            {
                Id = _numerator.Next,
                Status = AlertStatus.Pending,
                ProjectHealthRisk = projectHealthRisk,
                AlertReports = new List<AlertReport>()
            };
            var alertReports = reports.Select(r => new AlertReport
            {
                Report = r,
                ReportId = r.Id,
                Alert = newAlert,
                AlertId = newAlert.Id
            }).ToList();
            alertReports.ForEach(ar =>
            {
                newAlert.AlertReports.Add(ar);
                ar.Report.ReportAlerts.Add(ar);
            });

            return (new List<Alert> { newAlert }, alertReports);
        }
    }
}
