using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class SupervisorUserProjectMap : IEntityTypeConfiguration<SupervisorUserProject>
    {
        public void Configure(EntityTypeBuilder<SupervisorUserProject> builder)
        {
            builder.HasKey(sup => new { sup.SupervisorUserId, sup.ProjectId });
            builder.HasOne(uns => uns.SupervisorUser).WithMany(su => su.SupervisorUserProjects)
                .HasForeignKey(uns => uns.SupervisorUserId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(uns => uns.Project).WithMany(p => p.SupervisorUserProjects)
                .HasForeignKey(uns => uns.ProjectId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }
}
