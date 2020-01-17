using System.Collections.Generic;
using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.TestData.TestDataGeneration
{
    public class AlertGenerator
    {
        private readonly EntityNumerator _numerator = new EntityNumerator();

        public (List<RX.Nyss.Data.Models.Alert>, List<AlertReport>) AddPendingAlertForReports(List<Report> reports)
        {
            var projectHealthRisk = reports.First().ProjectHealthRisk;

            var newAlert = new RX.Nyss.Data.Models.Alert { Id = _numerator.Next, Status = AlertStatus.Pending, ProjectHealthRisk = projectHealthRisk };
            var alertReports = reports.Select(r => new AlertReport { Report = r, ReportId = r.Id, Alert = newAlert, AlertId = newAlert.Id }).ToList();
            alertReports.ForEach(ar => ar.Report.ReportAlerts.Add(ar));

            return (new List<RX.Nyss.Data.Models.Alert> { newAlert }, alertReports);
        }
    }
}
