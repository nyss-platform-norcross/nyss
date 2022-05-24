using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.DataCollectors.DataContracts
{
    public class ReplaceSupervisorData
    {
        public DataCollector DataCollector { get; set; }
        public SupervisorUser Supervisor { get; set; }
        public HeadSupervisorUser HeadSupervisor { get; set; }
        public RawReport LastReport { get; set; }
    }
}
