using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.TestData.TestDataGeneration
{
    public class LabeledReportGroup
    {
        private readonly EntityNumerator _reportNumerator;
        private readonly Guid _label;
        public DataCollector DataCollector { get; set; }

        public List<Report> Reports { get; set; }

        public LabeledReportGroup(EntityNumerator reportNumerator, Guid label)
        {
            _reportNumerator = reportNumerator;
            _label = label;
            Reports = new List<Report>();
        }

        public LabeledReportGroup AddReport(ReportStatus status, ProjectHealthRisk projectHealthRisk, DataCollector dataCollector, bool isTraining = false, Point location = null, DateTime? receivedAt = null, Village? village = null)
        {
            Reports.Add(new Report
            {
                Id = _reportNumerator.Next,
                ReportGroupLabel = _label,
                Status = status,
                ProjectHealthRisk = projectHealthRisk,
                DataCollector = dataCollector,
                IsTraining = isTraining,
                ReceivedAt = receivedAt ?? default,
                Location = location,
                Village = village ?? default
            });
            return this;
        }

        public LabeledReportGroup AddNReports(int numberOfReports, ReportStatus status, ProjectHealthRisk projectHealthRisk, DataCollector dataCollector, bool isTraining = false, Point location = null, DateTime ? receivedAt = null, Village? village = null)
        {
            Enumerable.Range(0,numberOfReports).ToList().ForEach(x => AddReport(status, projectHealthRisk, dataCollector, isTraining, location, receivedAt, village));
            return this;
        }
    }
}
