using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class RawReport
    {
        public int Id { get; set; }

        public string? Sender { get; set; }

        public string? Timestamp { get; set; }

        public DateTime ReceivedAt { get; set; }

        public string? Text { get; set; }

        public int? IncomingMessageId { get; set; }

        public int? OutgoingMessageId { get; set; }

        public int? ModemNumber { get; set; }

        public string ApiKey { get; set; }

        public int? ReportId { get; set; }

        public bool? IsTraining { get; set; }

        public DateTime? MarkedAsCorrectedAtUtc { get; set; }

        public string? MarkedAsCorrectedBy { get; set; }

        public ReportErrorType? ErrorType { get; set; }

        public virtual Village? Village { get; set; }

        public virtual Zone? Zone { get; set; }

        public virtual Report? Report { get; set; }

        public virtual DataCollector? DataCollector { get; set; }

        public virtual NationalSociety? NationalSociety { get; set; }
    }
}
