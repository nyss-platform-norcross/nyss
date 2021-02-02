using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class HeadSupervisorUserProjectMap : IEntityTypeConfiguration<HeadSupervisorUserProject>
    {
        public void Configure(EntityTypeBuilder<HeadSupervisorUserProject> builder)
        {
            builder.HasKey(x => new
            {
                x.HeadSupervisorUserId,
                x.ProjectId
            });
            builder
                .HasOne(hsup => hsup.HeadSupervisorUser)
                .WithMany(hsu => hsu.HeadSupervisorUserProjects)
                .HasForeignKey(hsup => hsup.HeadSupervisorUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(hsup => hsup.Project)
                .WithMany(p => p.HeadSupervisorUserProjects)
                .HasForeignKey(hsup => hsup.ProjectId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
