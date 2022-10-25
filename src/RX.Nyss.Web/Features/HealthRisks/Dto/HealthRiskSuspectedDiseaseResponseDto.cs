using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.HealthRisks.Dto
{
    public class HealthRiskSuspectedDiseaseResponseDto
    {
        public int SuspectedDiseaseId { get; set; }

        public int SuspectedDiseaseCode { get; set; }

        public string SuspectedDiseaseName { get; set; }

    }
}
