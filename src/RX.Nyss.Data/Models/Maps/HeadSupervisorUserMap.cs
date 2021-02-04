using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class HeadSupervisorUserMap : IEntityTypeConfiguration<HeadSupervisorUser>
    {
        public void Configure(EntityTypeBuilder<HeadSupervisorUser> builder)
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
                .HasColumnName("CurrentProjectId");

            builder.Property(u => u.ModemId)
                .HasColumnName("ModemId");

            builder.HasOne(u => u.CurrentProject)
                .WithMany()
                .IsRequired()
                .HasConstraintName("FK_Users_Project_CurrentProjectId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.Modem)
                .WithMany()
                .HasConstraintName("FK_Users_GatewayModem_GatewayModemId")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
