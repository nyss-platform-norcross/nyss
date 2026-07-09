using System.Collections.Generic;

namespace RX.Nyss.Web.Features.Agreements.Dto
{
    public class AgreementsStatusesDto
    {
        public IEnumerable<string> PendingSocieties { get; set; }
        public IEnumerable<string> StaleSocieties { get; set; }
    }
}
