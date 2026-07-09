using System.Collections.Generic;

namespace RX.Nyss.Web.Features.AlertEvents.Dto
{
    public class AlertEventCreateFormDto
    {
        public IEnumerable<AlertEventsTypeDto> EventTypes { get; set; } = new List<AlertEventsTypeDto>();
        public IEnumerable<AlertEventsSubtypeDto> EventSubtypes { get; set; } = new List<AlertEventsSubtypeDto>();
    }
}
