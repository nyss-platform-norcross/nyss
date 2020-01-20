using System;

namespace RX.Nyss.Web.Features.Projects.Dto
{
    public class ProjectListItemResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int TotalReportCount { get; set; }

        public int EscalatedAlertCount { get; set; }

        public int TotalDataCollectorCount { get; set; }

        public int SupervisorCount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
