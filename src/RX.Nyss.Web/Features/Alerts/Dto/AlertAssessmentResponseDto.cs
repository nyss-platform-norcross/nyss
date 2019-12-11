using System;
using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Alerts.Dto
{
    public class AlertAssessmentResponseDto
    {
        public string CaseDefinition { get; set; }

        public IEnumerable<ReportDto> Reports { get; set; }

        public IEnumerable<string> NotificationEmails { get; set; }

        public IEnumerable<string> NotificationPhoneNumbers { get; set; }

        public class ReportDto
        {
            public string PhoneNumber { get; set; }

            public string Village { get; set; }

            public Sex Sex { get; set; }

            public int? CountFemalesAtLeastFive { get; set; }

            public int? CountFemalesBelowFive { get; set; }

            public int? CountMalesAtLeastFive { get; set; }

            public int? CountMalesBelowFive { get; set; }

            public DateTime ReceivedAt { get; set; }

            public string Status { get; set; }
        }
    }
}
