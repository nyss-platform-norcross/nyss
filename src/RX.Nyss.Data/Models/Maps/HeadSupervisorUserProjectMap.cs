using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class LeadSupervisorUserProjectMap : IEntityTypeConfiguration<HeadSupervisorUserProject>
    {
        public void Configure(EntityTypeBuilder<HeadSupervisorUserProject> builder)
        {
            builder.HasKey(x => new
            {
                x.HeadSupervisorUserId,
                x.ProjectId
            });
            builder.HasOne(lsup => lsup.HeadSupervisorUser).WithMany(lsu => lsu.HeadSupervisorUserProjects)
                .HasForeignKey(lsup => lsup.HeadSupervisorUserId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(lsup => lsup.Project).WithMany(p => p.HeadSupervisorUserProjects)
                .HasForeignKey(lsup => lsup.ProjectId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }
}
