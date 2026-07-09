using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Resources.Dto
{
    public class GetStringResponseDto
    {
        public string Key { get; set; } = string.Empty;

        public bool NeedsImprovement { get; set; }

        public IEnumerable<GetEntryDto> Translations { get; set; } = new List<GetEntryDto>();

        public class GetEntryDto
        {
            public string LanguageCode { get; set; } = string.Empty;

            public string Value { get; set; } = string.Empty;

            public string Name { get; set; } = string.Empty;
        }
    }
}
