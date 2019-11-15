using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Resources.Dto
{
    public class GetStringResponseDto
    {
        public string Key { get; set; }

        public IEnumerable<EntryDto> Translations { get; set; }
    }
}
