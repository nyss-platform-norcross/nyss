using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class HeadSupervisorUser : User
    {
        public Sex Sex { get; set; }
        public int DecadeOfBirth { get; set; }
        public int CurrentProjectId { get; set; }
        public virtual ICollection<HeadSupervisorUserProject> HeadSupervisorUserProjects { get; set; } = new List<HeadSupervisorUserProject>();
        public virtual Project CurrentProject { get; set; }
        public virtual ICollection<HeadSupervisorUserAlertRecipient> HeadSupervisorUserAlertRecipients { get; set; }
    }
}
