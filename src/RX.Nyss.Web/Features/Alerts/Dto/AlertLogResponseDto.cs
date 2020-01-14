using System;
using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Alerts.Dto
{
    public class AlertLogResponseDto
    {
        public DateTime CreatedAt { get; set; }

        public string HealthRisk { get; set; }

        public IEnumerable<Item> Items { get; set; }

        public class Item
        {
            public Item(LogType logType, DateTime date, string userName, object metadata = null)
            {
                Date = date;
                LogType = logType;
                UserName = userName;
                Metadata = metadata;
            }

            public DateTime Date { get; set; }

            public LogType LogType { get; set; }

            public string UserName { get; }

            public object Metadata { get; }
        }

        public enum LogType
        {
            TriggeredAlert,
            EscalatedAlert,
            DismissedAlert,
            ClosedAlert,
            AcceptedReport,
            RejectedReport
        }
    }
}
