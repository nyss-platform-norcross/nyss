using System;
using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Project.Dto
{
    public class ProjectSummaryResponseDto
    {
        public DateTime StartDate { get; set; }

        public int ActiveDataCollectorCount { get; set; }

        public int InactiveDataCollectorCount { get; set; }

        public int InTrainingDataCollectorCount { get; set; }

        public IEnumerable<HealthRiskStats> HealthRisks { get; set; }

        public IEnumerable<SupervisorInfo> Supervisors { get; set; }

        public class HealthRiskStats
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public int TotalReportCount { get; set; }

            public int EscalatedAlertCount { get; set; }

            public int DismissedAlertCount { get; set; }
        }

        public class SupervisorInfo
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string EmailAddress { get; set; }

            public string PhoneNumber { get; set; }

            public string AdditionalPhoneNumber { get; set; }
        }
    }
}
