using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class SupervisorUserMap : IEntityTypeConfiguration<SupervisorUser>
    {
        public void Configure(EntityTypeBuilder<SupervisorUser> builder)
        {
            builder.HasBaseType<User>();
            builder.Property(u => u.Sex)
                .HasColumnName("Sex")
                .HasConversion<string>()
                .HasMaxLength(10)
                .IsRequired();
            builder.Property(u => u.DecadeOfBirth)
                .HasColumnName("DecadeOfBirth")
                .IsRequired();
            builder.Property(u => u.CurrentProjectId)
                .HasColumnName("CurrentProjectId")
                .IsRequired();
            builder.HasOne(u => u.CurrentProject)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(u => u.HeadSupervisor)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
