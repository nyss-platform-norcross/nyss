using NetTopologySuite.Geometries;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class Report
    {
        public int Id { get; set; }

        public ReportType ReportType { get; set; }

        public DateTime ReceivedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public DateTime? AcceptedAt { get; set; }

        public virtual User? AcceptedBy { get; set; }

        public DateTime? RejectedAt { get; set; }

        public virtual User? RejectedBy { get; set; }

        public DateTime? ResetAt { get; set; }

        public virtual User? ResetBy { get; set; }

        public string? ModifiedBy { get; set; }

        public ReportStatus Status { get; set; }

        public bool IsTraining { get; set; }

        public int EpiWeek { get; set; }

        public int EpiYear { get; set; }

        public string? PhoneNumber { get; set; }

        public Point? Location { get; set; }

        public int ReportedCaseCount { get; set; }

        public ReportCase ReportedCase { get; set; }

        public DataCollectionPointCase DataCollectionPointCase { get; set; }

        public Guid ReportGroupLabel { get; set; }

        public DateTime? CorrectedAt { get; set; }

        public virtual User? CorrectedBy { get; set; }

        public virtual RawReport RawReport { get; set; }

        public virtual ProjectHealthRisk? ProjectHealthRisk { get; set; }

        public virtual DataCollector? DataCollector { get; set; }

        public virtual ICollection<AlertReport> ReportAlerts { get; set; } = new List<AlertReport>();
    }
}
