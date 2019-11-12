using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class SupervisorUser : User
    {
        public Sex Sex { get; set; }
        public virtual ICollection<SupervisorUserProject> SupervisorUserProjects { get; set; } = new List<SupervisorUserProject>();
        public int DecadeOfBirth { get; set; }
    }
}
