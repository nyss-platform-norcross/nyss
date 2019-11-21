using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class RegionMap : IEntityTypeConfiguration<Region>
    {
        public void Configure(EntityTypeBuilder<Region> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(100);
            builder.HasOne(x => x.NationalSociety).WithMany().IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.Districts).WithOne(x => x.Region).IsRequired().OnDelete(DeleteBehavior.Cascade);
        }
    }
}
