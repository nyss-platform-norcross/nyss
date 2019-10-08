using System;
using NetTopologySuite.Geometries;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class Report
    {
        public int Id { get; set; }

        public ReportType ReportType { get; set; }
        
        public string RawContent { get; set; }
        
        public DateTime ReceivedAt { get; set; }
        
        public bool IsValid { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? ModifiedAt { get; set; }
        
        public string ModifiedBy { get; set; }

        public ReportStatus Status { get; set; }

        public bool IsTraining { get; set; }

        public Point Location { get; set; }

        public ReportCase ReportedCase { get; set; }

        public ReportCase KeptCase { get; set; }

        public DataCollectionPointCase DataCollectionPointCase { get; set; }

        public ProjectHealthRisk ProjectHealthRisk { get; set; }

        public DataCollector DataCollector { get; set; }
    }
}
