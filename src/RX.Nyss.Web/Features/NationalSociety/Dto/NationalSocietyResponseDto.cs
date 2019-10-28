using System;

namespace RX.Nyss.Web.Features.NationalSociety
{
    public class NationalSocietyResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }
        
        public int CountryId { get; set; }

        public string CountryName { get; set; }

        public int ContentLanguageId { get; set; }
    }
}
