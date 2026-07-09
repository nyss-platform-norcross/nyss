using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Resources.Dto
{
    public class ListTranslationsResponseDto
    {
        public List<LanguageResponseDto> Languages { get; set; } = new List<LanguageResponseDto>();
        public List<TranslationsResponseDto> Translations { get; set; } = new List<TranslationsResponseDto>();

        public class LanguageResponseDto
        {
            public string DisplayName { get; set; } = string.Empty;
            public string LanguageCode { get; set; } = string.Empty;
        }

        public class TranslationsResponseDto
        {
            public string Key { get; set; } = string.Empty;
            public bool NeedsImprovement { get; set; }
            public IDictionary<string, string> Translations { get; set; } = new Dictionary<string, string>();
        }
    }
}
