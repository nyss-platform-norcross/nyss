using System;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Project.Dto
{
    public class ProjectListItemResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int TotalReportCount { get; set; }

        public int EscalatedAlertCount { get; set; }

        public int ActiveDataCollectorCount { get; set; }

        public int SupervisorCount { get; set; }

        public ProjectState State { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
