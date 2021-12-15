using System;

namespace RX.Nyss.Data.Models
{
    public class ProjectErrorMessage
    {
        public int ProjectId { get; set; }

        public string MessageKey { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }

        public string UpdatedBy { get; set; }
    }
}
