using System.Collections.Generic;

namespace RX.Nyss.Web.Features.NationalSocieties.Dto
{
    public class PendingConsentDto
    {
        public List<PendingNationalSocietyConsentDto> NationalSocieties { get; set; }
        public IEnumerable<AgreementDocument> AgreementDocuments { get; set; }
    }

    public class AgreementDocument
    {
        public string Language { get; set; }

        public string AgreementDocumentUrl { get; set; }

        public string LanguageCode { get; set; }
    }

    public class PendingNationalSocietyConsentDto
    {
        public string NationalSocietyName { get; set; }
        public int NationalSocietyId { get; set; }
    }
}
