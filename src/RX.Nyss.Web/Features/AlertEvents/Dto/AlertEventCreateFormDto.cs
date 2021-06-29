using System.Collections.Generic;

namespace RX.Nyss.Web.Features.AlertEvents.Dto
{
    public class AlertEventCreateFormDto
    {
        public IEnumerable<AlertEventsTypeDto> EventTypes { get; set; }
        public IEnumerable<AlertEventsSubtypeDto> EventSubtypes { get; set; }
    }
}
