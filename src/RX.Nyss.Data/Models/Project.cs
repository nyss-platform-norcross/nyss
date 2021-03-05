using System;
using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public class Project
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ProjectState State { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int NationalSocietyId { get; set; }

        public bool AllowMultipleOrganizations { get; set; }

        public virtual NationalSociety NationalSociety { get; set; }

        public virtual ICollection<ProjectHealthRisk> ProjectHealthRisks { get; set; }

        public virtual ICollection<DataCollector> DataCollectors { get; set; }

        public virtual ICollection<SupervisorUserProject> SupervisorUserProjects { get; set; }

        public virtual ICollection<AlertNotificationRecipient> AlertNotificationRecipients { get; set; }

        public virtual ICollection<ProjectOrganization> ProjectOrganizations { get; set; }
        public virtual ICollection<HeadSupervisorUserProject> HeadSupervisorUserProjects { get; set; }
    }
}
