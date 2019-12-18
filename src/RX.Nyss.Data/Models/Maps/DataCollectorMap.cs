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
            builder.Property(x => x.DisplayName).HasMaxLength(100);
            builder.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
            builder.Property(x => x.AdditionalPhoneNumber).HasMaxLength(20);
            builder.Property(x => x.Location).IsRequired();
            builder.Property(x => x.Sex).HasConversion<string>().HasMaxLength(10);
            builder.Property(x => x.BirthGroupDecade);
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.DeletedAt);
            builder.HasOne(x => x.Project).WithMany(x => x.DataCollectors).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Supervisor).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Village).WithMany().IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Zone).WithMany().OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(x => x.Reports).WithOne(x => x.DataCollector).IsRequired().OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.PhoneNumber);
            builder.HasIndex(x => x.AdditionalPhoneNumber);
            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => x.DeletedAt);
            builder.HasIndex(x => x.DataCollectorType);
        }
    }
}
