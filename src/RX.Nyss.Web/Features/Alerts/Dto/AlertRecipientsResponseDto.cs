using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Alerts.Dto
{
    public class AlertRecipientsResponseDto
    {
        public IEnumerable<string> PhoneNumbers { get; set; }
        public IEnumerable<string> Emails { get; set; }
    }
}
