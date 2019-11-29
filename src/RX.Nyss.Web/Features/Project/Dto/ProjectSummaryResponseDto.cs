namespace RX.Nyss.Web.Features.Project.Dto
{
    public class ProjectSummaryResponseDto
    {
        public int ActiveDataCollectorCount { get; set; }

        public int InactiveDataCollectorCount { get; set; }

        public int InTrainingDataCollectorCount { get; set; }

        public int ReportCount { get; set; }
    }
}
