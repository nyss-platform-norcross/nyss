using System;

namespace RX.Nyss.Web.Features.Project.Dto
{
    public class ProjectListItemResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ReportCount { get; set; }

        public int EscalatedAlertCount { get; set; }

        public int ActiveDataCollectorCount { get; set; }

        public int SupervisorCount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
