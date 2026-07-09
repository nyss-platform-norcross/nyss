
namespace RX.Nyss.Web.Features.SuspectedDiseases.Dto
{
    public class SuspectedDiseaseListItemResponseDto
    {
        public int SuspectedDiseaseId { get; set; }

        public string SuspectedDiseaseName { get; set; } = string.Empty;

        public int SuspectedDiseaseCode { get; set; }
    }
}
