using System.Collections.Generic;

namespace RX.Nyss.Web.Features.NationalSociety.Dto
{
    public class GetPendingHeadManagerConsentsResponseDto
    {

        public List<PendingHeadManagerConsent> PendingHeadManagerConsents { get; set; }

        public class PendingHeadManagerConsent
        {
            public string NationalSocietyName { get; set; }
            public int NationalSocietyId { get; set; }
            public string NationalSocietyCountryName { get; set; }
        }
    }
}
