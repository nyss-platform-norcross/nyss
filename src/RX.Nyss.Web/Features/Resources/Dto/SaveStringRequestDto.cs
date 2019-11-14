using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Resources.Dto
{
    public class SaveStringRequestDto
    {
        public string Key { get; set; }

        public IEnumerable<EntryDto> Translations { get; set; }

        public class EntryDto
        {
            public string LanguageCode { get; set; }

            public string Value { get; set; }
        }
    }
}
