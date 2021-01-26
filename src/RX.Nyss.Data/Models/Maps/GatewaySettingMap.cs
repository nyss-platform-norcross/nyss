using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class GatewaySettingMap : IEntityTypeConfiguration<GatewaySetting>
    {
        public void Configure(EntityTypeBuilder<GatewaySetting> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.ApiKey)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.GatewayType)
                .HasConversion<string>()
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.EmailAddress)
                .HasMaxLength(100);

            builder.HasOne(x => x.NationalSociety)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.IotHubDeviceName)
                .HasMaxLength(250);

            builder.HasIndex(x => x.ApiKey)
                .IsUnique();
        }
    }
}
