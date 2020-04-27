using System;
using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Alerts.Dto
{
    public class AlertAssessmentResponseDto
    {
        public string HealthRisk { get; set; }

        public string CaseDefinition { get; set; }

        public IEnumerable<ReportDto> Reports { get; set; }

        public IEnumerable<AlertRecipientDto> NotificationRecipients { get; set; }

        public DateTime CreatedAt { get; set; }

        public AlertAssessmentStatus AssessmentStatus { get; set; }

        public CloseAlertOptions? CloseOption { get; set; }

        public string Comments { get; set; }

        public class AlertRecipientDto
        {
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
        }

        public class ReportDto
        {
            public int Id { get; set; }

            public string PhoneNumber { get; set; }

            public string Village { get; set; }

            public string Sex { get; set; }

            public DateTime ReceivedAt { get; set; }

            public string Status { get; set; }

            public string DataCollector { get; set; }

            public string Age { get; set; }
        }
    }
}
