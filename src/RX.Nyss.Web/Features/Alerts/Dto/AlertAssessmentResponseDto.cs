using System;
using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Alerts.Dto
{
    public class AlertAssessmentResponseDto
    {
        public string HealthRisk { get; set; }

        public bool IsNationalSocietyEidsrEnabled { get; set; }

        public string CaseDefinition { get; set; }

        public string Comments { get; set; }

        public bool RecipientsNotified { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? EscalatedAt { get; set; }

        public AlertAssessmentStatus AssessmentStatus { get; set; }

        public EscalatedAlertOutcomes? EscalatedOutcome { get; set; }

        public IEnumerable<ReportDto> Reports { get; set; }

        public IEnumerable<AlertAssessmentNotifiedUser> EscalatedTo { get; set; }

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

            public string Organization { get; set; }

            public string District { get; set; }

            public string Region { get; set; }

            public bool IsAnonymized { get; set; }
            public DateTime? AcceptedAt { get; set; }
            public DateTime? RejectedAt { get; set; }
            public DateTime? ResetAt { get; set; }
            public string SupervisorName { get; set; }
            public string SupervisorPhoneNumber { get; set; }
        }
    }
}
