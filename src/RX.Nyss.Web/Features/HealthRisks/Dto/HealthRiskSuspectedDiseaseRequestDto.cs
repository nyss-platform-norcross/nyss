using System.ComponentModel.DataAnnotations;

namespace RX.Nyss.Web.Features.HealthRisks.Dto
{
    public class HealthRiskSuspectedDiseaseRequestDto
    {
        [Required]
        public int SuspectedDiseaseId { get; set; }
    }
}
