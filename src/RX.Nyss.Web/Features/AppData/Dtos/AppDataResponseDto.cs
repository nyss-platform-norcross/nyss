using System.Collections.Generic;

namespace RX.Nyss.Web.Features.AppData.Dtos
{
    public class AppDataResponseDto
    {
        public List<ContentLanguageDto> ContentLanguages { get; set; }

        public List<CountryDto> Countries { get; set; }

        public class ContentLanguageDto
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public class CountryDto
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
