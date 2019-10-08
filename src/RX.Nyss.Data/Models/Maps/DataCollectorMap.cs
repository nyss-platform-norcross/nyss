using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class DataCollectorMap : IEntityTypeConfiguration<DataCollector>
    {
        public void Configure(EntityTypeBuilder<DataCollector> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.DataCollectorType).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
            builder.Property(x => x.AdditionalPhoneNumber).HasMaxLength(20);
            builder.Property(x => x.Location).IsRequired();
            builder.HasOne(x => x.Project).WithMany().IsRequired();
            builder.HasOne(x => x.Supervisor).WithMany().IsRequired();
            builder.HasOne(x => x.Village).WithMany().IsRequired();
            builder.HasOne(x => x.Zone);
        }
    }
}
