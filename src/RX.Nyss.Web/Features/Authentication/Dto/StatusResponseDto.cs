using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Authentication.Dto
{
    public class StatusResponseDto
    {
        public bool IsAuthenticated { get; set; }
        public DataDto Data { get; set; }


        public class DataDto
        {
            public string Name { get; set; }
            
            public string Email { get; set; }

            public string[] Roles { get; set; }

            public string LanguageCode { get; set; }

            public List<PendingHeadManagerConsent> PendingHeadManagerConsents { get; set; }

            public class PendingHeadManagerConsent
            {
                public string NationalSocietyName { get; set; }

                public int NationalSocietyId { get; set; }
            }

            public HomePageDto HomePage { get; set; }
        }

        public class HomePageDto
        {
            public HomePageType Page { get; set; }
            public int? NationalSocietyId { get; set; }
            public int? ProjectId { get; set; }
        }
    }
}
