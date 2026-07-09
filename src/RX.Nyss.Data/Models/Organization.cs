using System.Collections.Generic;

namespace RX.Nyss.Data.Models
{
    public class Organization
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NationalSocietyId { get; set; }
        public virtual NationalSociety NationalSociety { get; set; }
        public virtual ICollection<UserNationalSociety> NationalSocietyUsers { get; set; }
        public virtual ICollection<ProjectOrganization> OrganizationProjects { get; set; }
        public int? HeadManagerId { get; set; }
        public virtual User HeadManager { get; set; }
        public int? PendingHeadManagerId { get; set; }
        public virtual User PendingHeadManager { get; set; }
    }
}
