using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class ProjectOrganizationMap : IEntityTypeConfiguration<ProjectOrganization>
    {
        public void Configure(EntityTypeBuilder<ProjectOrganization> builder)
        {
            builder.HasKey(uns => uns.Id);

            builder.HasIndex(uns => new
            {
                uns.ProjectId,
                uns.OrganizationId
            }).IsUnique();

            builder.HasOne(uns => uns.Project).WithMany(u => u.ProjectOrganizations).HasForeignKey(uns => uns.ProjectId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(uns => uns.Organization).WithMany(o => o.OrganizationProjects).HasForeignKey(uns => uns.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
