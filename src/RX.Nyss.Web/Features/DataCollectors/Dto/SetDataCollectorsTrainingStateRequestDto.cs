using System.Collections.Generic;

namespace RX.Nyss.Web.Features.DataCollectors.Dto
{
    public class SetDataCollectorsTrainingStateRequestDto
    {
        public IEnumerable<int> DataCollectorIds { get; set; } = new List<int>();

        public bool InTraining { get; set; }
    }
}
