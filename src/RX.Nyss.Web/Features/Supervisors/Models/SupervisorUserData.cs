using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.Supervisors.Models
{
    public class SupervisorUserData
    {
        public SupervisorUser User { get; set; } = default!;
        public SupervisorUserProject CurrentProjectReference { get; set; } = new SupervisorUserProject();
    }
}