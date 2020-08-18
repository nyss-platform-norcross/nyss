using System;
using System.Collections.Generic;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class ReplaceSupervisorRequestDto
    {
        public IEnumerable<int> DataCollectorIds { get; set; }
        public int SupervisorId { get; set; }
    }
}
