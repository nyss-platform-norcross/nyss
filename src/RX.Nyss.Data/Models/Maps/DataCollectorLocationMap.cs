using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class DataCollectorLocationMap : IEntityTypeConfiguration<DataCollectorLocation>
    {
        public void Configure(EntityTypeBuilder<DataCollectorLocation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Location)
                .IsRequired();

            builder.HasOne(x => x.DataCollector)
                .WithMany(x => x.DataCollectorLocations)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Village)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Zone)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
