namespace RX.Nyss.Data.Models
{
    public class ProjectOrganization
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public Project? Project { get; set; }

        public int OrganizationId { get; set; }

        public Organization? Organization { get; set; }
    }
}
