using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.HeadSupervisors.Models
{
    public class HeadSupervisorUserData
    {
        public HeadSupervisorUser? User { get; set; }
        public HeadSupervisorUserProject? CurrentProjectReference { get; set; }
    }
}