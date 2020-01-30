using System;
using NetTopologySuite.Geometries;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.TestData.TestDataGeneration
{
    public class ReportGenerator
    {
        private readonly EntityNumerator _numerator = new EntityNumerator();

        public Report CreateNewReport(ProjectHealthRisk projectHealthRisk, DataCollector dataCollector, bool isTraining = false, Point location = null, DateTime? receivedAt = null)
        {
            return new Report
            {
                Id = _numerator.Next,
                Status = ReportStatus.New,
                ProjectHealthRisk = projectHealthRisk,
                DataCollector = dataCollector,
                IsTraining = isTraining,
                ReceivedAt = receivedAt ?? default,
                Location = location
            };
        }
    }
}
