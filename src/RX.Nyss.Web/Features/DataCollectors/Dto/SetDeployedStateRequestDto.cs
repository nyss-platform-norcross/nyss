using System.Collections.Generic;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class SetDeployedStateRequestDto
    {
        public IEnumerable<int> DataCollectorIds { get; set; }
        public bool Deployed { get; set; }
    }
}
