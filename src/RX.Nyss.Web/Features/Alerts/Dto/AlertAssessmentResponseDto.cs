using System;
using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Alerts.Dto
{
    public class AlertAssessmentResponseDto
    {
        public string HealthRisk { get; set; } = string.Empty;

        public bool IsNationalSocietyEidsrEnabled { get; set; }

        public string CaseDefinition { get; set; } = string.Empty;

        public int HealthRiskCountThreshold { get; set; }

        public int? HealthRiskDaysThreshold { get; set; }

        public int? HealthRiskKilometersThreshold { get; set; }

        public string? Comments { get; set; }

        public bool RecipientsNotified { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? EscalatedAt { get; set; }

        public AlertAssessmentStatus AssessmentStatus { get; set; }

        public EscalatedAlertOutcomes? EscalatedOutcome { get; set; }

        public IEnumerable<ReportDto> Reports { get; set; } = new List<ReportDto>();

        public IEnumerable<AlertAssessmentNotifiedUser> EscalatedTo { get; set; } = new List<AlertAssessmentNotifiedUser>();

        public class ReportDto
        {
            public int Id { get; set; }

            public string PhoneNumber { get; set; } = string.Empty;

            public string Village { get; set; } = string.Empty;

            public string? Zone { get; set; }

            public string Sex { get; set; } = string.Empty;

            public DateTime ReceivedAt { get; set; }

            public string Status { get; set; } = string.Empty;

            public string DataCollector { get; set; } = string.Empty;

            public string Age { get; set; } = string.Empty;

            public string Organization { get; set; } = string.Empty;

            public string District { get; set; } = string.Empty;

            public string Region { get; set; } = string.Empty;

            public bool IsAnonymized { get; set; }
            public DateTime? AcceptedAt { get; set; }
            public DateTime? RejectedAt { get; set; }
            public DateTime? ResetAt { get; set; }
            public string SupervisorName { get; set; } = string.Empty;
            public string SupervisorPhoneNumber { get; set; } = string.Empty;
        }
    }
}
