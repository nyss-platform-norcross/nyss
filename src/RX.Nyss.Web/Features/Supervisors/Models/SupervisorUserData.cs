using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.Supervisors.Models
{
    public class SupervisorUserData
    {
        public SupervisorUser User { get; set; }
        public SupervisorUserProject CurrentProjectReference { get; set; }
    }
}