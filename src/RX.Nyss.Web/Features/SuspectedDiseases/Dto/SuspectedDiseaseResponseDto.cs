using System.Collections.Generic;

namespace RX.Nyss.Web.Features.SuspectedDiseases.Dto
{
    public class SuspectedDiseaseResponseDto
    {
        public int SuspectedDiseaseId { get; set; }

        public int SuspectedDiseaseCode { get; set; }

        public IEnumerable<SuspectedDiseaseLanguageContentDto> LanguageContent { get; set; }
    }
}
