using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Resources.Dto
{
    public class DeleteStringRequestDto
    {
        public string Key { get; set; }
        public IEnumerable<DeleteEntryDto> Translations { get; set; }
        public class DeleteEntryDto
        {
            public string LanguageCode { get; set; }

            public string Value { get; set; }
        }

    }



}
