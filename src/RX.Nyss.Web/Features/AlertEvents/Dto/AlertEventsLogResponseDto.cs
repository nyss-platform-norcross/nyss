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
        public string HealthRisk { get; set; }
        public IEnumerable<LogItem> LogItems { get; set; }
        public DateTime CreatedAt { get; set; }

        public class LogItem
        {
            public LogType? LogType { get; set; }
            public DateTime Date { get; set; }
            public string AlertEventType { get; set; }
            public string AlertEventSubtype { get; set; }
            public string LoggedBy { get; set; }
            public string Text { get; set; }
            public object Metadata { get; set; }
        }
    }
}
