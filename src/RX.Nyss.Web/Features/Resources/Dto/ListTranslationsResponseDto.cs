using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Resources.Dto
{
    public class ListTranslationsResponseDto
    {
        public List<LanguageResponseDto> Languages { get; set; }
        public List<TranslationsResponseDto> Translations { get; set; }

        public class LanguageResponseDto
        {
            public string DisplayName { get; set; }
            public string LanguageCode { get; set; }
        }

        public class TranslationsResponseDto
        {
            public string Key { get; set; }
            public IDictionary<string, string> Translations { get; set; }
        }
    }
}
