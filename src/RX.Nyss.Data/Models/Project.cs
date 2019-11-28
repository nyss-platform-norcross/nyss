using System;
using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class Project
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string TimeZone { get; set; }

        public ProjectState State { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int NationalSocietyId { get; set; }

        public virtual NationalSociety NationalSociety { get; set; }

        public virtual ICollection<ProjectHealthRisk> ProjectHealthRisks { get; set; }

        public virtual ICollection<DataCollector> DataCollectors { get; set; }

        public virtual ICollection<AlertRecipient> AlertRecipients { get; set; }
        public virtual ICollection<SupervisorUserProject> SupervisorUserProjects { get; set; }
    }
}
