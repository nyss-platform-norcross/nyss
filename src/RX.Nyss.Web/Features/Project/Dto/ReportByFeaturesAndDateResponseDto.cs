namespace RX.Nyss.Web.Features.Project.Dto
{
    public class ReportByFeaturesAndDateResponseDto
    {
        public string Period { get; set; }

        public int CountFemalesAtLeastFive { get; set; }

        public int CountFemalesBelowFive { get; set; }

        public int CountMalesAtLeastFive { get; set; }

        public int CountMalesBelowFive { get; set; }
    }
}
