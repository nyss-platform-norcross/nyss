using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Agreements.Dto
{
    public class PendingConsentDto
    {
        public List<PendingNationalSocietyConsentDto> PendingSocieties { get; set; }
        public List<PendingNationalSocietyConsentDto> StaleSocieties { get; set; }
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
