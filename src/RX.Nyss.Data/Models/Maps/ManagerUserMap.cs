using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class ManagerUserMap : IEntityTypeConfiguration<ManagerUser>
    {
        public void Configure(EntityTypeBuilder<ManagerUser> builder)
        {
            builder.HasBaseType<User>();

            builder.Property(u => u.ModemId)
                .HasColumnName("ModemId");

            builder.HasOne(u => u.Modem)
                .WithMany()
                .HasConstraintName("FK_Users_GatewayModem_GatewayModemId")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
