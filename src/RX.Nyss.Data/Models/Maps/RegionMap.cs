using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RX.Nyss.Data.Models.Maps
{
    public class RegionMap : IEntityTypeConfiguration<Region>
    {
        public void Configure(EntityTypeBuilder<Region> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.NationalSociety).WithMany().IsRequired();
            builder.Property(x => x.Name).HasMaxLength(100);
        }
    }
}
