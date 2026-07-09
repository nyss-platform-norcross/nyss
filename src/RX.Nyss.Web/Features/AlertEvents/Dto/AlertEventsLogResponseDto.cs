using System;
using System.Collections.Generic;

namespace RX.Nyss.Web.Features.AlertEvents.Dto
{
    public class AlertEventsLogResponseDto
    {
        public enum LogType
        {
            TriggeredAlert,
            EscalatedAlert,
            DismissedAlert,
            ClosedAlert,
            AcceptedReport,
            RejectedReport,
            ResetReport
        }
        public string HealthRisk { get; set; } = string.Empty;
        public IEnumerable<LogItem> LogItems { get; set; } = new List<LogItem>();
        public DateTime CreatedAt { get; set; }

        public class LogItem
        {
            public int? AlertEventLogId { get; set; }
            public LogType? LogType { get; set; }
            public DateTime Date { get; set; }
            public string AlertEventType { get; set; } = string.Empty;
            public string AlertEventSubtype { get; set; } = string.Empty;
            public string LoggedBy { get; set; } = string.Empty;
            public string Text { get; set; } = string.Empty;
            public object Metadata { get; set; } = new { };
        }
    }
}
