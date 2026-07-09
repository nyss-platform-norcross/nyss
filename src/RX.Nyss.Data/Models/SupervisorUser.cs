using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class SupervisorUser : User
    {
        public Sex Sex { get; set; }
        public int DecadeOfBirth { get; set; }
        public int CurrentProjectId { get; set; }
        public int? ModemId { get; set; }
        public virtual ICollection<SupervisorUserProject> SupervisorUserProjects { get; set; } = new List<SupervisorUserProject>();
        public virtual Project CurrentProject { get; set; }
        public virtual ICollection<SupervisorUserAlertRecipient>? SupervisorAlertRecipients { get; set; }
        public virtual HeadSupervisorUser? HeadSupervisor { get; set; }
        public virtual GatewayModem? Modem { get; set; }
    }
}
